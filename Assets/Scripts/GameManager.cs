using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour {

    public static GameManager inatance;

    public Transform[] playersPos;
    public Text[] playerResult;
    public Transform usedPos;
    public GameObject cardPrefab;
    public Image choiceCardPrefab;
    public CanvasGroup choiceCardGroup;
    public CanvasGroup resultGroup;

    public List<GameObject> usedDeck = new List<GameObject>();

    private int takeCardIndex = -1;

    private List<GameObject> gameDeck = new List<GameObject>();
    private List<Player> players = new List<Player>();
    private int playerIndex = 0;
    private int giverPlayerIndex = 0;
    private int loserIndex = 0;

    float scale;
    float width;
    float height;

    private List<Image> choiseCards = new List<Image>();

    private IEnumerator corDealCard;
    //private IEnumerator corGetCardIndex;
    private bool isDealing = false;
    private bool isGameover = false;
    private bool isWaiting = false;
    private bool isChangePlayer = false;
    private bool isStart = false;

    void Awake()
    {
        inatance = this;
    }

    void Start()
    {
        scale = (choiceCardGroup.GetComponentInParent<RectTransform>().rect.width / 8f) / choiceCardPrefab.preferredWidth;
        width = (choiceCardPrefab.preferredWidth + 20) * scale;
        height = (choiceCardPrefab.preferredHeight + 20) * scale;

        corDealCard = SendIngCard();
        usedDeck = new List<GameObject>();

        for (int i = 0; i < playersPos.Length; i++)
        {
            Player player = new Player();
            player.playerPos = playersPos[i];
            player.playerIndex = i;
            players.Add(player);
        }
        CreateDeck(cardPrefab, 1, 1);

        Restart();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (gameDeck.Count == 0 && isDealing)
        {
            isDealing = false;
            StopCoroutine(corDealCard);
            ArrangeCardsAll();

            for (int i = 0; i < players.Count; i++)
            {
                usedDeck = players[i].RemoveRepeat(usedDeck, usedPos.localPosition);
            }
            playerIndex = loserIndex;
            isChangePlayer = true;
            isStart = true;
        }

        if (!isDealing && !isGameover)
        {
            StartCoroutine(PlayTurn());
        }
    }

    public void DealCard(bool isUpdate = true)
    {
        isDealing = true;
        StartCoroutine(corDealCard);
    }

    IEnumerator SendIngCard(bool isUpdate = true)
    {
        while (gameDeck.Count > 0)
        {
            playerIndex = playerIndex % players.Count;
            GameObject dealCard = gameDeck[0];
            Card card = gameDeck[0].GetComponent<Card>();
            Vector3 targetPos = players[playerIndex].playerPos.localPosition;
            card.settingPos = new Vector3(targetPos.x, targetPos.y, players[playerIndex].cards.Count * -0.01f);
            card.setToPos = true;
            players[playerIndex].cards.Add(dealCard);
            gameDeck.RemoveAt(0);
            playerIndex++;
            yield return new WaitForSeconds(0.2f);
        }
    }

    IEnumerator PlayTurn()
    {
        if (isStart)
        {
            yield return new WaitForSeconds(1);
            isStart = false;
        }
        if (!isWaiting)
        {
            if (isChangePlayer)
            {
                giverPlayerIndex = GetLastPlayerIndex(playerIndex);
                while (giverPlayerIndex != playerIndex && players[giverPlayerIndex].cards.Count == 0)
                {
                    giverPlayerIndex = GetLastPlayerIndex(giverPlayerIndex);
                }
                isChangePlayer = false;
            }

            if (giverPlayerIndex == playerIndex)
            {
                //game over
                isGameover = true;
                loserIndex = playerIndex;
                yield return new WaitForSeconds(1);
                if (!players[playerIndex].cards[0].GetComponent<Card>().GetIsFaceOn)
                {
                    players[playerIndex].cards[0].GetComponent<Card>().TurnCard();
                }
                yield return new WaitForSeconds(2);
                EnableResult();
            }
            else
            {
                if (takeCardIndex < 0)
                {
                    //Take Card
                    if (playerIndex == 0)
                    {
                        //Player chooice
                        CreateChoiseCard(players[giverPlayerIndex].cards.Count);
                        EnableChoise();
                        isWaiting = true;
                    }
                    else
                    {
                        takeCardIndex = Random.Range(0, players[giverPlayerIndex].cards.Count - 1);
                    }
                }
                else
                {
                    if (playerIndex == 0)
                    {
                        DisableChoise();
                        RemoveChoiseCards();
                    }
                    isWaiting = true;

                    TakeCard();

                    yield return new WaitForSeconds((playerIndex + giverPlayerIndex) % 2 == 0 ? 2 : 1);
                    players[giverPlayerIndex].ArrangeCards();

                    usedDeck = players[playerIndex].RemoveRepeat(usedDeck, usedPos.localPosition);
                    players[playerIndex].cards = Shuffle(players[playerIndex].cards);
                    players[playerIndex].ArrangeCards();

                    yield return new WaitForSeconds(2);
                    SetTakeCardIndex();
                    NextTurn();
                }
            }
        }
    }

    public void SetTakeCardIndex(int index = -1)
    {
        takeCardIndex = index;
        isWaiting = false;
    }

    public void CreateDeck(GameObject cardPrefab, int deckCounts, int jokerCounts = 0)
    {
        for (int i = 0; i < deckCounts; i++)
        {
            gameDeck.AddRange(CreateSingleDeck(cardPrefab));
        }
        gameDeck = AddJokerToDeck(gameDeck, cardPrefab, jokerCounts);

        gameDeck = Shuffle(gameDeck);
    }

    private void NextTurn()
    {
        int tempIndex = playerIndex;
        do
        {
            tempIndex++;
            tempIndex = tempIndex % players.Count;
            if (tempIndex == playerIndex)
            {
                break;
            }
        } while (players[tempIndex].cards.Count == 0);
        playerIndex = tempIndex;
        isChangePlayer = true;
    }

    private void ArrangeCardsAll()
    {
        for (int i = 0; i < players.Count; i++)
        {
            if (i==0)
            {
                players[i].TurnCardAll();
            }
            players[i].SetCardsRotation();
            players[i].ArrangeCards();
        }
    }

    private int GetLastPlayerIndex(int index)
    {
        return (index -1 + players.Count) % players.Count;
    }

    private void CreateChoiseCard(int cardCount)
    {
        float totalWeight = width * cardCount;
        if (cardCount > 5)
        {
            totalWeight = width * 5;
        }
        for (int i = 0; i < cardCount; i++)
        {
            Image card = Instantiate(choiceCardPrefab);
            card.transform.SetParent(choiceCardGroup.transform);
            card.transform.localScale = new Vector3(scale, scale, 1);
            card.transform.localPosition = new Vector3((-totalWeight / 2) + ((i % 5) * width) + (width / 2), cardCount <= 5 ? 0 : i < 5 ? (height) / 2 : -(height) / 2, 0);
            card.GetComponent<ChoiseCard>().cardIndex = i;
            choiseCards.Add(card);
        }
    }

    private void TakeCard()
    {
        GameObject dealCard = players[giverPlayerIndex].cards[takeCardIndex];
        Card selectedCard = dealCard.GetComponent<Card>();
        Vector3 targetPos = players[playerIndex].playerPos.localPosition;
        selectedCard.settingPos = new Vector3(targetPos.x, targetPos.y, players[playerIndex].cards.Count * -0.01f);
        selectedCard.setToPos = true;
        players[playerIndex].cards.Add(dealCard);
        players[giverPlayerIndex].cards.RemoveAt(takeCardIndex);
        if (playerIndex == 0 || giverPlayerIndex == 0)
        {
            selectedCard.TurnCard();
        }
        if ((giverPlayerIndex + playerIndex) % 2 == 1)
        {
            dealCard.transform.Rotate(0, 0, 90);
        }

    }

    private void RemoveChoiseCards()
    {
        for (int i = choiseCards.Count - 1; i >= 0; i--)
        {
            Image temp = choiseCards[i];
            choiseCards.RemoveAt(i);
            Destroy(temp.gameObject);
        }
    }

    private void EnableChoise()
    {
        choiceCardGroup.alpha = 1;
        choiceCardGroup.blocksRaycasts = true;
        choiceCardGroup.interactable = true;
    }

    private void DisableChoise()
    {
        choiceCardGroup.alpha = 0;
        choiceCardGroup.blocksRaycasts = false;
        choiceCardGroup.interactable = false;
    }

    private void EnableResult()
    {
        for (int i = 0; i < players.Count; i++)
        {
            playerResult[i].text = players[i].cards.Count == 0 ? "Win" : "Lose";
            playerResult[i].color = players[i].cards.Count == 0 ? new Color(255, 174, 0) : new Color(104, 0, 255);
        }

        resultGroup.alpha = 1;
        resultGroup.blocksRaycasts = true;
        resultGroup.interactable = true;
    }

    private void DisableResult()
    {
        resultGroup.alpha = 0;
        resultGroup.blocksRaycasts = false;
        resultGroup.interactable = false;
    }

    private List<GameObject> CreateSingleDeck(GameObject cardPrefab)
    {
        List<GameObject> deck = new List<GameObject>();
        for (int i = 1; i < 53; i++)
        {
            string name = "";
            switch ((i-1) / 13)
            {
                case 0:
                    name = "Spades" + ((i - 1) % 13 + 1);
                    break;
                case 1:
                    name = "Heart" + ((i - 1) % 13 + 1);
                    break;
                case 2:
                    name = "Diamond" + ((i - 1) % 13 + 1);
                    break;
                case 3:
                    name = "Club" + ((i - 1) % 13 + 1);
                    break;
                default:
                    name = "Joker";
                    break;
            }

            GameObject card = (GameObject)Instantiate(cardPrefab, new Vector3(0, 0, 0), Quaternion.Euler(new Vector3()));
            card.name = name;
            Card cardScript = card.GetComponent<Card>();
            cardScript.cardIndex = i;
            cardScript.SetCardFace();
            cardScript.TurnCard();
            deck.Add(card);
        }

        return deck;
    }

    private List<GameObject> AddJokerToDeck(List<GameObject> deck, GameObject cardPrefab, int jokerCounts)
    {
        for (int i = 0; i < jokerCounts; i++)
        {
            GameObject card = (GameObject)Instantiate(cardPrefab, new Vector3(0, 0, 0), Quaternion.Euler(new Vector3()));
            card.name = "Joker";
            Card cardScript = card.GetComponent<Card>();
            cardScript.cardIndex = 0;
            cardScript.SetCardFace();
            cardScript.TurnCard();
            deck.Add(card);
        }

        return deck;
    }

    private static List<T> Shuffle<T>(IEnumerable<T> values)
    {
        List<T> list = new List<T>(values);
        T tmp;
        int iS;
        for (int N1 = 0; N1 < list.Count; N1++)
        {
            iS = Random.Range(0, list.Count);
            tmp = list[N1];
            list[N1] = list[iS];
            list[iS] = tmp;
        }
        return list;
    }

    private void ResetDeckPos()
    {
        for (int i = 0; i < gameDeck.Count; i++)
        {
            gameDeck[i].GetComponent<Card>().ResetCard();

            if (gameDeck[i].GetComponent<Card>().GetIsFaceOn)
            {
                gameDeck[i].GetComponent<Card>().TurnCard();
            }
            gameDeck[i].transform.localPosition = new Vector3(usedPos.position.x, usedPos.position.y, (i * 0.01f) - (gameDeck.Count * 0.01f));
        }
    }

    public void Restart()
    {
        DisableChoise();
        DisableResult();
        isGameover = false;
        isWaiting = false;
        isChangePlayer = false;
        isDealing = true;

        for (int i = 0; i <= players.Count; i++)
        {
            List<GameObject> temp = new List<GameObject>();

            if (i!= players.Count)
            {
                temp = players[i].cards;
            }
            else
            {
                temp = usedDeck;
            }

            for (int j = temp.Count - 1; j >= 0; j--)
            {
                gameDeck.Add(temp[j]);
                temp.RemoveAt(j);
            }
        }
        gameDeck = Shuffle(gameDeck);
        ResetDeckPos();

        playerIndex = loserIndex;
        DealCard();
    }

    public void ToTitle()
    {
        SceneManager.LoadScene("Menu", LoadSceneMode.Single);
    }
}
