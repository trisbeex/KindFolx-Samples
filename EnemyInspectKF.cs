using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyInspectKF : MonoBehaviour
{
    public BoxCollider2D[] colliders;
    Vector3 startingScale;
    bool inspecting; //used to determine if the player has stopped on an enemy or the mouse grazed by it
    bool inspectOperations; // used to determine if the buttons are expanding/contracting, if so, let it finish

    //used to determine if an enemy is off the ground for specific cases like Semmy's ground pound
    public bool airborne = false;
    Target target;
    public GameObject targetSprite;
    public GameObject targetLoot;
    public GameObject targetIcon;
    public GameObject actionMeterTargetIcon;
    public GameObject enemyAlert;
    public GameObject enemyInspect;
    public GameObject inspectIcon;
    public GameObject dimmer;

    //the object that houses all the enemy telegraphs so they can be added on top of the dimmer when inspected
    public GameObject enemyTelegraphs;
    public GameObject enemyStatBar;
    public GameObject damageParticles;

    CardDraw cardDraw;
    Navigation navigation;
    bool alerted = false;

    public Transform damageArea;

    // Position Storage Variables (Hover Variables Cont.)
    Vector3 posOffset = new Vector3();
    Vector3 offSetValue = new Vector3(0f, 0f, 0f);
    Vector3 tempPos = new Vector3();

    float amplitude = 40f;
    float frequency = .9f;

    EnemyEncounter enemyEncounter;

    void Start()
    {
        startingScale = this.transform.localScale;
        navigation = GameAssets.i.navigation;
        cardDraw = GameAssets.i.cardDraw;
        target = GameAssets.i.target;
        posOffset = targetIcon.transform.position + offSetValue;
        enemyEncounter = this.transform.parent.parent.GetComponent<EnemyEncounter>();

    }

    private void Update()
    {
        if (cardDraw.drawing && !alerted && this.GetComponent<EnemyActions>().enabled == true)
        {
            StartCoroutine(AlertEnemy());
            alerted = true;
        }

        if (target.currentTarget == this.gameObject && !navigation.moving) //cardDraw.drawing == true
        {

            // Float up/down with a Sin()
            tempPos = posOffset;
            tempPos.x += Mathf.Sin(Time.fixedTime * Mathf.PI * frequency) * amplitude;

            targetIcon.transform.position = tempPos + offSetValue;
        }
    }

    public void EndHover()
    {
        StopCoroutine(HoverEffect());
        StartCoroutine(HoverEffectEnd());
    }

    private void OnMouseExit()
    {
        StopCoroutine(HoverEffect());
        StartCoroutine(HoverEffectEnd());
    }

    private void OnMouseEnter()
    {

        //if you are not inspecting any enemy or this is the enemy you're inspecting, perform mouse hover over effects on the enemy
        if (!inspecting && !GameAssets.i.universalVariables.inspectingEnemy || inspecting)
        {
            //due to issues with onmousexit not always firing we will manually reduce the size of any other enemies that may still be enlargened
            foreach (GameObject enemy in enemyEncounter.victoryRequirements)
            {
                if (enemy != this.gameObject)
                {
                    enemy.GetComponentInChildren<Targetable>().EndHover();
                }

            }

            StartCoroutine(HoverEffect());
        }
    }

    private void OnMouseOver()
    {

        if (Input.GetMouseButtonDown(1) && !inspecting && !GameAssets.i.universalVariables.inspectingEnemy && GameAssets.i.playerMoves.placeholders.Count == 0)
        {
            //this prevents other enemy's colliders from getting in the way of the current inspect
            foreach (GameObject enemy in enemyEncounter.victoryRequirements)
            {
                if (enemy != this.gameObject)
                {
                    enemy.GetComponentInChildren<Targetable>().gameObject.GetComponent<BoxCollider2D>().enabled = false;
                }

            }

            PrepareInspect();
        }

    }

    //if the mouse is clicked and released on the enemy it becomes the target
    private void OnMouseUpAsButton()
    {

        if (GameAssets.i.handCanvas.inspectPlaceholder != null)
        {
            // Click was on a UI element — ignore
            return;
        }

        //if the player isn't moving and isn't inspecting an enemy allow a target change
        if (!navigation.moving && !GameAssets.i.universalVariables.inspectingEnemy)
        {
            bool taunted = false;

            if (target.currentTarget.GetComponent<StatusEffects>().taunt)
            {
                target.SelectTarget(target.currentTarget);
                taunted = true;
            }

            if (!taunted)
            {
                target.SelectTarget(this.gameObject);
                StartCoroutine(TargetEffect());
            }

            else
            {
                PlayerAudio.PlayTauntedPhrase();
            }
        }

        else if (GameAssets.i.universalVariables.inspectingEnemy && inspecting) // else if this enemy is being inspected and clicked exit
        {
            Dimmerclick();
        }

    }

    public void ChangeTarget()
    {
        //if the player isn't moving and isn't inspecting an enemy allow a target change
        if (!navigation.moving && !GameAssets.i.universalVariables.inspectingEnemy)
        {
            bool taunted = false;
            if (target.currentTarget.GetComponent<StatusEffects>().taunt)
            {
                taunted = true;
            }

            if (!taunted)
            {
                target.SelectTarget(this.gameObject);
                StartCoroutine(TargetEffect());
            }
        }

        else if (GameAssets.i.universalVariables.inspectingEnemy && inspecting) // else if this enemy is being inspected and clicked exit
        {
            Dimmerclick();
        }
    }

    public void PrepareInspect()
    {

        GameAssets.i.target.canToggleInspect = false;
        inspectIcon.transform.position =
            new Vector3(gameObject.transform.position.x + 2, gameObject.transform.position.y, gameObject.transform.position.z);

        Debug.Log("Right click worked");
        if (!inspectOperations)
        {
            StopCoroutine(BattleBeginZoomIn());
        }

        //allows player to inspect even during combat

        //only allows inspect to happen after battle begin button is displayed
        if (cardDraw.cardPrep && !cardDraw.inspecting && cardDraw.battleBeginButton)
        {
            GameAssets.i.universalVariables.inspectingEnemy = true;
            Debug.Log("Inspecting after button is shown");
            StartCoroutine(BeginInspect());


        }

        // allows inspect after combat started
        else if (cardDraw.cardPrep && !cardDraw.inspecting && cardDraw.drawing)

        {
            GameAssets.i.universalVariables.inspectingEnemy = true;
            Debug.Log("Inspecting after combat");
            StartCoroutine(BeginInspect());

        }
    }

    IEnumerator BeginInspect()
    {
        yield return new WaitUntil(() => !GameAssets.i.cardDraw.drawingNewHand);
        Debug.Log("Expecting enemy");
        //turn off animations for all but the enemy being inspected

        GetComponent<Animator>().updateMode = AnimatorUpdateMode.UnscaledTime;

        GameAssets.i.cardDraw.canvasBlocker.SetActive(true);
        dimmer.SetActive(true);

        Vector3 originalScale1 = dimmer.transform.localScale;
        Vector3 destinationScale1 = new Vector3(43, 33, 1f);

        float currentTime1 = 0.0f;

        do
        {
            dimmer.transform.localScale = Vector3.Lerp(originalScale1, destinationScale1, currentTime1 / .05f);
            currentTime1 += Time.deltaTime;
            yield return null;
        } while (currentTime1 <= .15); //.15 is the time rate

        this.GetComponent<SpriteRenderer>().sortingLayerName = "Top";
        this.targetSprite.GetComponent<SpriteRenderer>().sortingLayerName = "Top";
        enemyStatBar.GetComponent<Canvas>().sortingOrder = 11;
        enemyTelegraphs.GetComponent<SortingGroup>().sortingOrder = 1;
        //turn off the player's collider so it doesn't block any inspect elements
        GameAssets.i.playerState.GetComponent<BoxCollider2D>().enabled = false;

        if (target.currentTarget == this.gameObject)
        {
            targetIcon.GetComponent<SpriteRenderer>().sortingOrder = 1;
            targetIcon.GetComponent<SpriteRenderer>().sortingLayerName = "Top";
        }

        inspecting = true;
        StopCoroutine(InspectOut());
        StartCoroutine(InspectZoom());

    }
    public IEnumerator AlertEnemy()
    {
        this.gameObject.GetComponent<BoxCollider2D>().enabled = true; //allows the player to target
        enemyAlert.GetComponent<SpriteRenderer>().enabled = true;

        Vector3 originalScale1 = enemyAlert.transform.localScale;
        Vector3 destinationScale1 = new Vector3(1f, .8336634f, 1f);

        float currentTime1 = 0.0f;

        do
        {
            enemyAlert.transform.localScale = Vector3.Lerp(originalScale1, destinationScale1, currentTime1 / .05f);
            currentTime1 += Time.deltaTime;
            yield return null;
        } while (currentTime1 <= .15); //.15 is the time rate

        Vector3 originalScale = enemyAlert.transform.localScale;
        Vector3 destinationScale = originalScale1;

        float currentTime = 0.0f;

        do
        {
            enemyAlert.transform.localScale = Vector3.Lerp(originalScale, destinationScale, currentTime / .05f);
            currentTime += Time.deltaTime;
            yield return null;
        } while (currentTime <= .15);

        enemyAlert.GetComponent<SpriteRenderer>().enabled = false;

        Destroy(enemyAlert);
    }

    public void SetTargetIcon()
    {
        foreach (GameObject enemy in this.transform.parent.parent.GetComponent<EnemyEncounter>().victoryRequirements)
        {
            enemy.GetComponent<EnemyDataHolder>().enemyObjectReference.GetComponent<Targetable>().targetIcon.SetActive(false);
            enemy.GetComponent<EnemyDataHolder>().enemyObjectReference.GetComponent<Targetable>().actionMeterTargetIcon.SetActive(false);
        }

        actionMeterTargetIcon.SetActive(true);
        targetIcon.SetActive(true);
    }

    public void Target()
    {
        StartCoroutine(TargetEffect());
    }

    public IEnumerator TargetEffect()
    {

        SetTargetIcon();

        targetSprite.GetComponent<SpriteRenderer>().enabled = true;
        Vector3 originalScale1 = targetSprite.transform.localScale;
        Vector3 destinationScale1 = new Vector3(1.3f, 1.3f, 1.3f);

        float time = .15f;
        float currentTime1 = 0.0f;

        do
        {
            targetSprite.transform.localScale = Vector3.Lerp(originalScale1, destinationScale1, currentTime1 / .05f);
            currentTime1 += Time.deltaTime;
            yield return null;
        } while (currentTime1 <= time);

        Vector3 originalScale = targetSprite.transform.localScale;

        float currentTime = 0.0f;

        do
        {
            targetSprite.transform.localScale = Vector3.Lerp(originalScale, originalScale1, currentTime / time);
            currentTime += Time.deltaTime;
            yield return null;
        } while (currentTime <= time);

        targetSprite.GetComponent<SpriteRenderer>().enabled = false;

    }

    IEnumerator InspectZoom()
    {

        if (inspecting)
        {
            GameAssets.i.cardDraw.gameObject.SetActive(false);

            inspectIcon.SetActive(true);
            Vector3 originalScale = inspectIcon.transform.localScale;
            Vector3 destinationScale = new Vector3(.07f, .07f, 1f);


            float currentTime = 0.0f;

            do
            {
                inspectIcon.transform.localScale = Vector3.Lerp(originalScale, destinationScale, currentTime / .05f);
                currentTime += Time.deltaTime;
                yield return null;
            } while (currentTime <= .15); //.15 is the time rate

            enemyInspect.transform.localScale = new Vector3(0, 0, 0);
            Vector3 originalScale1 = enemyInspect.transform.localScale;
            Vector3 destinationScale1 = new Vector3(.6f, .6f, 0f);

            float time = .10f;
            float currentTime1 = 0.0f;

            Vector3 originalScale2 = GameAssets.i.cardDraw.battleBeginButton.gameObject.transform.localScale;
            Vector3 destinationScale2 = new Vector3(.0f, .0f, 0f);


            enemyInspect.transform.position = enemyInspect.GetComponent<EnemyInspect>().inspectMarker.transform.position;
            enemyInspect.SetActive(true);

            FMODUnity.RuntimeManager.PlayOneShot("event:/Sounds/UI/enemy inspect open");

            do
            {
                yield return null;

                enemyInspect.transform.localScale = Vector3.Lerp(originalScale1, destinationScale1, currentTime1 / .05f);
                GameAssets.i.cardDraw.battleBeginButton.transform.localScale = Vector3.Lerp(originalScale2, destinationScale2, currentTime1 / .05f);
                currentTime1 += Time.deltaTime;
                yield return null;
            } while (currentTime1 <= time);
        }

        yield return null;
        dimmer.GetComponent<BoxCollider2D>().enabled = false;
        dimmer.GetComponent<BoxCollider2D>().enabled = true;
        //stop all animations except the inspected enemy
        Time.timeScale = 0;

        // allow the player to push the button again to inspect
        GameAssets.i.target.canToggleInspect = true;

    }

    IEnumerator InspectOut()
    {

        if (!inspecting)
        {

            FMODUnity.RuntimeManager.PlayOneShot("event:/Sounds/UI/enemy inspect close");
            enemyInspect.transform.localScale = new Vector3(0, 0, 0);
            Vector3 originalScale1 = enemyInspect.transform.localScale;
            Vector3 destinationScale1 = new Vector3(.0f, .0f, 0f);

            float time = .10f;
            float currentTime1 = 0.0f;

            Vector3 originalScale2 = GameAssets.i.cardDraw.battleBeginButton.gameObject.transform.localScale;
            Vector3 destinationScale2 = new Vector3(.01f, .01f, .01f);


            enemyInspect.transform.position = this.transform.parent.GetComponentInChildren<EnemyInspect>().inspectMarker.transform.position;
            enemyInspect.SetActive(true);


            do
            {
                yield return null;

                enemyInspect.transform.localScale = Vector3.Lerp(originalScale1, destinationScale1, currentTime1 / .05f);
                GameAssets.i.cardDraw.battleBeginButton.gameObject.transform.localScale = Vector3.Lerp(originalScale2, destinationScale2, currentTime1 / .05f);
                currentTime1 += Time.deltaTime;
                yield return null;
            } while (currentTime1 <= time);
        }


        Time.timeScale = 1;
        Debug.Log("Restored time scale");
        GetComponent<Animator>().updateMode = AnimatorUpdateMode.Normal;
    }

    IEnumerator BattleBeginZoom()
    {

        if (inspecting && !inspectOperations)
        {
            inspectOperations = true;

            Vector3 originalScale1 = GameAssets.i.cardDraw.battleBeginButton.gameObject.transform.localScale;
            Vector3 destinationScale1 = new Vector3(.0f, .0f, 0f);

            float time = .10f;
            float currentTime1 = 0.0f;

            do
            {
                yield return null;
                GameAssets.i.cardDraw.battleBeginButton.gameObject.transform.localScale = Vector3.Lerp(originalScale1, destinationScale1, currentTime1 / .05f);
                currentTime1 += Time.deltaTime;
                yield return null;
            } while (currentTime1 <= time);

            inspectOperations = false;

            GameAssets.i.cardDraw.battleBeginButton.gameObject.SetActive(false);
        }

    }

    IEnumerator BattleBeginZoomIn()
    {

        if (!inspecting && !inspectOperations)
        {
            GameAssets.i.cardDraw.gameObject.SetActive(true);
            inspectOperations = true;
            GameAssets.i.cardDraw.battleBeginButton.SetActive(true);

            Vector3 originalScale1 = GameAssets.i.cardDraw.battleBeginButton.transform.localScale;
            Vector3 destinationScale1 = new Vector3(.01f, .01f, .01f);

            float time = .10f;
            float currentTime1 = 0.0f;

            do
            {
                yield return null;
                GameAssets.i.cardDraw.battleBeginButton.transform.localScale = Vector3.Lerp(originalScale1, destinationScale1, currentTime1 / .05f);
                currentTime1 += Time.deltaTime;
                yield return null;
            } while (currentTime1 <= time);
            inspectOperations = false;

        }

    }


    public void Dimmerclick()
    {
        //this prevents other enemy's colliders from getting in the way of the current inspect
        foreach (GameObject enemy in enemyEncounter.victoryRequirements)
        {
            if (enemy != this.gameObject)
            {
                enemy.GetComponentInChildren<Targetable>().gameObject.GetComponent<BoxCollider2D>().enabled = true;
            }

        }

        //prevent the player from inspecting again before the inspect ends
        GameAssets.i.target.canToggleInspect = false;
        StartCoroutine(EndInspect());
    }

    //closes the inspect window
    IEnumerator EndInspect()
    {
        GameAssets.i.cardDraw.canvasBlocker.SetActive(false);
        GameAssets.i.universalVariables.inspectingEnemy = false;

        //restore animations to normal
        Time.timeScale = 1;
        Debug.Log("Restored time scale");
        GetComponent<Animator>().updateMode = AnimatorUpdateMode.Normal;

        inspecting = false;

        if (!inspectOperations)
        {
            StopCoroutine(BattleBeginZoom());
        }

        StopCoroutine(InspectZoom());
        StartCoroutine(CloseInfo());

        Vector3 originalScale1 = dimmer.transform.localScale;
        Vector3 destinationScale1 = new Vector3(0, 0, 1f);

        float currentTime1 = 0.0f;

        do
        {
            dimmer.transform.localScale = Vector3.Lerp(originalScale1, destinationScale1, currentTime1 / .05f);
            currentTime1 += Time.deltaTime;
            yield return null;
        } while (currentTime1 <= .15); //.15 is the time rate

        GameAssets.i.levelSetup.levelDimmer.SetActive(false);

        GameAssets.i.cardDraw.gameObject.SetActive(true);
        StartCoroutine(InspectOut());
        enemyInspect.SetActive(false);

        this.targetSprite.GetComponent<SpriteRenderer>().sortingLayerName = "Default";
        this.GetComponent<SpriteRenderer>().sortingLayerName = "Default";

        if (!navigation.moving && !cardDraw.drawing)
        {

            Debug.Log("Disabling inspect");
            enemyInspect.SetActive(false);

            if (!cardDraw.drawing)
            {
                GameAssets.i.cardDraw.battleBeginButton.SetActive(true);
            }

        }

        enemyStatBar.GetComponent<Canvas>().sortingOrder = -3;
        enemyTelegraphs.GetComponent<SortingGroup>().sortingOrder = -3;
        GameAssets.i.playerState.GetComponent<BoxCollider2D>().enabled = true;

        GameAssets.i.target.canToggleInspect = true;
    }

    //shrinks the inspect window at the same time as the background
    IEnumerator CloseInfo()
    {

        Vector3 originalScale1 = enemyInspect.transform.localScale;
        Vector3 destinationScale1 = new Vector3(0, 0, 0);

        float time = .10f;
        float currentTime1 = 0.0f;

        Vector3 originalScale2 = GameAssets.i.cardDraw.battleBeginButton.gameObject.transform.localScale;
        Vector3 destinationScale2 = new Vector3(.0f, .0f, 0f);

        enemyInspect.transform.position = enemyInspect.GetComponent<EnemyInspect>().inspectMarker.transform.position;
        enemyInspect.SetActive(true);

        FMODUnity.RuntimeManager.PlayOneShot("event:/Sounds/UI/enemy inspect open");

        do
        {
            yield return null;

            enemyInspect.transform.localScale = Vector3.Lerp(originalScale1, destinationScale1, currentTime1 / .05f);
            GameAssets.i.cardDraw.battleBeginButton.gameObject.transform.localScale = Vector3.Lerp(originalScale2, destinationScale2, currentTime1 / .05f);
            currentTime1 += Time.deltaTime;
            yield return null;
        } while (currentTime1 <= time);

        Vector3 originalScale = inspectIcon.transform.localScale;
        Vector3 destinationScale = new Vector3(0, 0, 1f);

        float currentTime = 0.0f;

        do
        {
            inspectIcon.transform.localScale = Vector3.Lerp(originalScale, destinationScale, currentTime / .05f);
            currentTime += Time.deltaTime;
            yield return null;
        } while (currentTime <= .15); //.15 is the time rate
        inspectIcon.SetActive(false);
        inspectIcon.transform.position = gameObject.transform.position;

    }

    IEnumerator HoverEffect()
    {

        Vector3 originalScale1 = startingScale;
        Vector3 destinationScale1 = new Vector3(1.2f, 1.2f, 1.2f);

        float time = .10f;
        float currentTime1 = 0.0f;

        do
        {
            this.transform.localScale = Vector3.Lerp(originalScale1, destinationScale1, currentTime1 / .05f);
            currentTime1 += Time.deltaTime;
            yield return null;

        } while (currentTime1 <= time);

    }

    IEnumerator HoverEffectEnd()
    {

        Vector3 originalScale1 = this.transform.localScale;
        Vector3 destinationScale1 = startingScale;

        float time = .10f;
        float currentTime1 = 0.0f;

        do
        {

            this.transform.localScale = Vector3.Lerp(originalScale1, destinationScale1, currentTime1 / .05f);
            currentTime1 += Time.deltaTime;
            yield return null;

        } while (currentTime1 <= time);

    }

}
