using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Player
{
    public List<GameObject> cards = new List<GameObject>();
    public Transform playerPos;
    public float offsetWeight = 0.4f;
    public float offsetHeight = 0.02f;
    public int playerIndex;

    private static float pokerWeight = 1.4f;

    public void TurnCardAll()
    {
        for (int i = 0; i < cards.Count; i++)
        {
            cards[i].GetComponent<Card>().TurnCard();
        }
    }

    public void SetCardsRotation()
    {
        for (int i = 0; i < cards.Count; i++)
        {
            cards[i].transform.Rotate(0, 0, (playerIndex % 2) * 90);
        }
    }

    public void ArrangeCards()
    {
        float iniLeft = -(((cards.Count - 1) * offsetWeight + pokerWeight) / 2) + (pokerWeight / 2);
        for (int i = 0; i < cards.Count; i++)
        {
            if (playerIndex % 2==0)
            {
                cards[i].GetComponent<Card>().settingPos = new Vector3((iniLeft + playerPos.localPosition.x + i * offsetWeight) * (playerIndex / 2 == 0 ? 1 : -1), 0 + playerPos.localPosition.y, i * -offsetHeight);
            }
            else
            {
                cards[i].GetComponent<Card>().settingPos = new Vector3(0 + playerPos.localPosition.x, (iniLeft + playerPos.localPosition.y + i * offsetWeight) * (playerIndex / 2 == 0 ? -1 : 1), i * -offsetHeight);
            }
            cards[i].GetComponent<Card>().setToPos = true;
        }
    }

    public List<GameObject> RemoveRepeat(List<GameObject> usedDeck,Vector3 usedPos)
    {
        List<int> repeatIndex = CheckRepeat();

        while (repeatIndex.Count > 0)
        {
            repeatIndex.Sort();
            repeatIndex.Reverse();
            for (int i = 0; i < 2; i++)
            {
                Card card = cards[repeatIndex[i]].GetComponent<Card>();
                if (!card.GetIsFaceOn)
                {
                    card.TurnCard();
                }
                card.settingPos = new Vector3(usedPos.x, usedPos.x, usedDeck.Count * -0.01f);
                card.setToPos = true;

                usedDeck.Add(cards[repeatIndex[i]]);
                cards.RemoveAt(repeatIndex[i]);
            }
            repeatIndex = CheckRepeat();
        }
        ArrangeCards();

        return usedDeck;
    }

    private List<int> CheckRepeat()
    {
        List<int> repeatIndex = new List<int>();

        for (int i = 0; i < cards.Count - 1; i++)
        {
            for (int j = i + 1; j < cards.Count; j++)
            {
                if (cards[i].GetComponent<Card>().naturePoint == cards[j].GetComponent<Card>().naturePoint)
                {
                    repeatIndex.Add(i);
                    repeatIndex.Add(j);
                    break;
                }
            }
            if (repeatIndex.Count>0)
            {
                break;
            }
        }

        return repeatIndex;
    }

}
