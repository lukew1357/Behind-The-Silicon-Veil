using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PlayerDataScript : MonoBehaviour, IDataPersistence
{
    private int coins = 0;
    private int health = 3;
    private int deathCount = 0;
    public TextMeshProUGUI coinsText;
    public TextMeshProUGUI deathCountText;

    public void LoadData(GameData data)
    {
        this.coins = data.coins;
        this.health = data.health;
        this.deathCount = data.deathCount;
    }
    public void SaveData(GameData data)
    {
        data.coins = this.coins;
        data.health = this.health;
        data.deathCount = this.deathCount;
    }

    // Start is called before the first frame update
    void Start()
    {
        GameEventsManager.instance.onCoinCollected += OnCoinCollected;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.C))
        {
            coins++;
        }
        if (Input.GetKeyDown(KeyCode.KeypadMinus))
        {
            deathCount++;
        }
        deathCountText.text = "Deaths: " + deathCount;
        coinsText.text = "Coins: " + coins;
    }

    private void OnCoinCollected()
    {
        coins++;
    }
}
