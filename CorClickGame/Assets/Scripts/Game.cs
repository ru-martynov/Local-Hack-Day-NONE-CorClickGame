
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;
using System;

public class Game : MonoBehaviour
{
    [Header("Текст, отвечающий за отображение денег")]
    public Text scoreText;
    [Header("Магазин")]
    public List<Item> shopItems = new List<Item>();
    [Header("Текст на кнопках товаров")]
    public Text[] shopItemsText;
    [Header("Кнопки товаров")]
    public Button[] shopBttns;
    [Header("Панелька магазина")]
    public GameObject shopPan;

    private int score; 
    private int scoreIncrease = 1; 
    private Save sv = new Save();

    private void Awake()
    {
        if (PlayerPrefs.HasKey("SV"))
        {
            int totalBonusPS = 0;
            sv = JsonUtility.FromJson<Save>(PlayerPrefs.GetString("SV"));
            score = sv.score;
            for (int i = 0; i < shopItems.Count; i++)
            {
                shopItems[i].levelOfItem = sv.levelOfItem[i];
                shopItems[i].bonusCounter = sv.bonusCounter[i];
                if (shopItems[i].needCostMultiplier) shopItems[i].cost *= (int)Mathf.Pow(shopItems[i].costMultiplier, shopItems[i].levelOfItem);
                if (shopItems[i].bonusIncrease != 0 && shopItems[i].levelOfItem != 0) scoreIncrease += (int)Mathf.Pow(shopItems[i].bonusIncrease, shopItems[i].levelOfItem);
                totalBonusPS += shopItems[i].bonusPerSec * shopItems[i].bonusCounter;
            }
            DateTime dt = new DateTime(sv.date[0], sv.date[1], sv.date[2], sv.date[3], sv.date[4], sv.date[5]);
            TimeSpan ts = DateTime.Now - dt;
            int offlineBonus = (int)ts.TotalSeconds * totalBonusPS;
            score += offlineBonus;
            print("Вы отсутствовали: " + ts.Days + "Д. " + ts.Hours + "Ч. " + ts.Minutes + "М. " + ts.Seconds + "S.");
            print("Иммунитет повысился" + offlineBonus);
        }
    }

    private void Start()
    {
        updateCosts(); 
        StartCoroutine(BonusPerSec()); 
    }

    private void Update()
    {
        scoreText.text = score + ""; 
    }

    public void BuyBttn(int index) 
    {
        int cost = shopItems[index].cost * shopItems[shopItems[index].itemIndex].bonusCounter; 
        if (shopItems[index].itsBonus && score >= cost) 
        {
            if (cost > 0) 
            {
                score -= cost; 
                StartCoroutine(BonusTimer(shopItems[index].timeOfBonus, index)); 
            }
            else print("Нечего улучшать то!"); 
        }
        else if (score >= shopItems[index].cost) 
        {
            if (shopItems[index].itsItemPerSec) shopItems[index].bonusCounter++; 
            else scoreIncrease += shopItems[index].bonusIncrease; 
            score -= shopItems[index].cost; 
            if (shopItems[index].needCostMultiplier) shopItems[index].cost *= shopItems[index].costMultiplier; 
            shopItems[index].levelOfItem++; 
        }
        else print("Не хватает Иммунитета!"); 
        updateCosts(); 
    }
    private void updateCosts() 
    {
        for (int i = 0; i < shopItems.Count; i++) 
        {
            if (shopItems[i].itsBonus) 
            {
                int cost = shopItems[i].cost * shopItems[shopItems[i].itemIndex].bonusCounter; 
                shopItemsText[i].text = shopItems[i].name + "\n" + cost + " Иммунитета"; 
            }
            else shopItemsText[i].text = shopItems[i].name + "\n" + shopItems[i].cost + " Иммунитета"; 
        }
    }

    IEnumerator BonusPerSec() 
    {
        while (true) 
        {
            for (int i = 0; i < shopItems.Count; i++) score += (shopItems[i].bonusCounter * shopItems[i].bonusPerSec); 
            yield return new WaitForSeconds(1); 
        }
    }
    IEnumerator BonusTimer(float time, int index) 
    {
        shopBttns[index].interactable = false; 
        shopItems[shopItems[index].itemIndex].bonusPerSec *= 2; 
        yield return new WaitForSeconds(time); 
        shopItems[shopItems[index].itemIndex].bonusPerSec /= 2; 
        shopBttns[index].interactable = true; 
    }
#if UNITY_ANDROID && !UNITY_EDITOR
    private void OnApplicationPause(bool pause)
    {
        if (pause)
        {
            sv.score = score;
            sv.levelOfItem = new int[shopItems.Count];
            sv.bonusCounter = new int[shopItems.Count];
            for (int i = 0; i < shopItems.Count; i++)
            {
                sv.levelOfItem[i] = shopItems[i].levelOfItem;
                sv.bonusCounter[i] = shopItems[i].bonusCounter;
            }
            sv.date[0] = DateTime.Now.Year; sv.date[1] = DateTime.Now.Month; sv.date[2] = DateTime.Now.Day; sv.date[3] = DateTime.Now.Hour; sv.date[4] = DateTime.Now.Minute; sv.date[5] = DateTime.Now.Second;
            PlayerPrefs.SetString("SV", JsonUtility.ToJson(sv));
        }
    }
#else
    private void OnApplicationQuit()
    {
        sv.score = score;
        sv.levelOfItem = new int[shopItems.Count];
        sv.bonusCounter = new int[shopItems.Count];
        for (int i = 0; i < shopItems.Count; i++)
        {
            sv.levelOfItem[i] = shopItems[i].levelOfItem;
            sv.bonusCounter[i] = shopItems[i].bonusCounter;
        }
        sv.date[0] = DateTime.Now.Year; sv.date[1] = DateTime.Now.Month; sv.date[2] = DateTime.Now.Day; sv.date[3] = DateTime.Now.Hour; sv.date[4] = DateTime.Now.Minute; sv.date[5] = DateTime.Now.Second;
        PlayerPrefs.SetString("SV", JsonUtility.ToJson(sv));
    }
#endif

    public void showShopPan() 
    {
        shopPan.SetActive(!shopPan.activeSelf); 
    }

    public void OnClick() 
    {
        score += scoreIncrease; 
    }

}
[Serializable]
public class Item 
{
    [Tooltip("Название используется на кнопках")]
    public string name;
    [Tooltip("Цена товара")]
    public int cost;
    [Tooltip("Бонус, который добавляется к бонусу при клике")]
    public int bonusIncrease;
    [HideInInspector]
    public int levelOfItem; 
    [Space]
    [Tooltip("Нужен ли множитель для цены?")]
    public bool needCostMultiplier;
    [Tooltip("Множитель для цены")]
    public int costMultiplier;
    [Space]
    [Tooltip("Этот товар даёт бонус в секунду?")]
    public bool itsItemPerSec;
    [Tooltip("Бонус, который даётся в секунду")]
    public int bonusPerSec;
    [HideInInspector]
    public int bonusCounter; 
    [Space]
    [Tooltip("Это временный бонус?")]
    public bool itsBonus;
    [Tooltip("Множитель товара, который управляется бонусом (Умножается переменная bonusPerSec)")]
    public int itemMultiplier;
    [Tooltip("Индекс товара, который будет управляться бонусом (Умножается переменная bonusPerSec этого товара)")]
    public int itemIndex;
    [Tooltip("Длительность бонуса")]
    public float timeOfBonus;
}
[Serializable]
public class Save
{
    public int score;
    public int[] levelOfItem;
    public int[] bonusCounter;
    public int[] date = new int[6];
}