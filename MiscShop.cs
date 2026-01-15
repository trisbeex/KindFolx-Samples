using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MiscShop : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    public GameObject itemDescription;
    public GameObject product;
    public int price;
    public bool patchUp;
    public Image purchasedIcon;
    public bool purchased;

    private void Start()
    {
        //if the items were purchased already during the last session, mark them as sold out
        if (patchUp && ES3.KeyExists("patchup"))
        {
            purchased = true;
            purchasedIcon.enabled = true;
        }

        if (!patchUp && ES3.KeyExists("purified"))
        {
            purchased = true;
            purchasedIcon.enabled = true;
        }
    }

    //purchase the item if the player can afford it and in stock
    public void OnPointerClick(PointerEventData eventData)
    {
        if (GameAssets.i.playerGold.goldHeld >= price && !purchased)
        {
            purchased = true;
            purchasedIcon.enabled = true;
            GameAssets.i.playerGold.RemoveGold(price);

            if (patchUp)
            {
                GameAssets.i.playerStats.Heal(25);
                SaveOperations.instance.SavePatchUp();
            }

            else
            {
                GameAssets.i.cardDraw.playerDeck.Add(product);
                GameAssets.i.cardDraw.baseDeck.Add(product);
                SaveOperations.instance.SavePurified();
            }
        }



        else if (!purchased)
        {
            GetComponent<DialogueTrigger>().TriggerDialogue();
        }
        

    }

    //display the item description on mouse hover
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!purchased)
        {
            itemDescription.transform.parent.SetAsLastSibling();
            itemDescription.SetActive(true);
        }
        
    }

    //remove the item description on mouse exit
    public void OnPointerExit(PointerEventData eventData)
    {

        itemDescription.SetActive(false);
    }
}
