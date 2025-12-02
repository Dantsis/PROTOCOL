using UnityEngine;
using TMPro;

public class AmmoUI : MonoBehaviour
{
    public PlayerShoot playerShoot;
    public TextMeshProUGUI ammoText;

    void Start()
    {
        if (playerShoot == null)
            playerShoot = FindObjectOfType<PlayerShoot>();

        if (ammoText == null)
            ammoText = GetComponent<TextMeshProUGUI>();
    }

    void Update()
    {
        if (playerShoot == null || ammoText == null) return;

        ammoText.text = playerShoot.currentAmmo + " / " + playerShoot.maxAmmo;
    }
}
