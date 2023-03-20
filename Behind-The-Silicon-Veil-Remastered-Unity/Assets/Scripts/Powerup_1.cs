using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Powerup_1 : MonoBehaviour, IDataPersistence
{
    [SerializeField] private string id;

    [ContextMenu("Generate guid for id")]
    private void GenerateGuid()
    {
        id = System.Guid.NewGuid().ToString();
    }

    private bool collected = false;
    private SpriteRenderer visual;

    public void LoadData(GameData data)
    {
        data.coinsCollected.TryGetValue(id, out collected);
        if (collected)
        {
            visual.gameObject.SetActive(false);
        }
    }

    public void SaveData(GameData data)
    {
        if (data.coinsCollected.ContainsKey(id))
        {
            data.coinsCollected.Remove(id);
        }
        data.coinsCollected.Add(id, collected);
    }

    private void Awake()
    {
        visual = this.GetComponent<SpriteRenderer>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Player")
        {
            GameObject player = collision.gameObject;
            PlayerMovementScript playerScript = player.GetComponent<PlayerMovementScript>();

            if (playerScript)
            {
                Debug.Log("powerup 1 collected");
                Destroy(gameObject);
            }
        }
    }
}
