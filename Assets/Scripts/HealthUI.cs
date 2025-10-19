using NUnit.Framework;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class HealthUI : MonoBehaviour
{
    [SerializeField] private Image heartPrefab;
    [SerializeField] private Sprite fullContainer;
    [SerializeField] private Sprite emptyContainer;

    private List<Image> hearts = new List<Image>();

    public void SetMaxHearts(int maxHearts)
    {
        foreach (Image heart in hearts)
        {
            Destroy(heart.gameObject);
        }

        hearts.Clear();

        for(int i = 0; i < maxHearts; i++)
        {
            Image newHeart = Instantiate(heartPrefab, transform);
            newHeart.sprite = fullContainer;
            hearts.Add(newHeart);
        }
    }

    public void UpdateHearts(int currentHearts)
    {
        for (int i = 0; i < hearts.Count; i++)
        {
            if(i < currentHearts)
            {
                hearts[i].sprite = fullContainer;
            }
            else
            {
                hearts[i].sprite = emptyContainer;
            }
        }
    }
}
