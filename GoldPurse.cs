using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoldPurse : MonoBehaviour
{

    public int goldHeld;
    Particles particles;

    private void Start()
    {
        particles = GameAssets.i.particles;
    }


    //increases enemy gold loot
    public void AddGoldEnemy(int gold)
    {
        FMODUnity.RuntimeManager.PlayOneShot("event:/Sounds/purchase");
        DisplayDamage.CreatePopUpGold(this.gameObject.transform.position, gold, .6f, .9f);
        particles.PlayEffect(particles.goldSparkle, GetComponent<StatusEffects>().particleArea.position);
        goldHeld += gold;
    }


    //adds gold to the player incrementally and plays an effect at their position
    public void AddGold(int gold)
    {
        StartCoroutine(AddGoldFlow(gold));

    }

    IEnumerator AddGoldFlow(int gold)
    {
        FMODUnity.RuntimeManager.PlayOneShot("event:/Sounds/purchase");
        DisplayDamage.CreatePopUpGoldZoom(this.transform.position, gold, 0.6f, 0.9f);
        particles.PlayEffect(particles.goldSparkle, GetComponent<StatusEffects>().particleArea.position);

        int remainingToAdd = gold;

        while (remainingToAdd > 0)
        {
            goldHeld++;
            remainingToAdd--;
            yield return null;
        }

        if (!GameAssets.i.navigation.encounter)
        {
            SaveOperations.instance.SavePlayer();
        }
    }

    //removes the current gold from the character and plays an effect at their position
    public void RemoveGold(int gold)
    {
        StartCoroutine(RemoveGoldFlow(gold));
    }

    IEnumerator RemoveGoldFlow(int gold)
    {
        FMODUnity.RuntimeManager.PlayOneShot("event:/Sounds/purchase");
        DisplayDamage.CreatePopUpGold(GameAssets.i.playerState.transform.position, -gold, 0.6f, 0.9f);
        particles.PlayEffect(particles.goldSparkle, GetComponent<StatusEffects>().particleArea.position);

        int remainingToRemove = gold;

        while (remainingToRemove > 0)
        {
            goldHeld--;
            remainingToRemove--;

            if (goldHeld < 0)
            {
                goldHeld = 0;
                break;
            }

            yield return null;
        }
    }
}
