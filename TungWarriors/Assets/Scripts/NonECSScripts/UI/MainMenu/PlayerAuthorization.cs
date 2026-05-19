using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using YG;

public class PlayerAuthorization : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI playerName;
    [SerializeField] private TextMeshProUGUI playerId;
    [SerializeField] private ImageLoadYG playerImage;
    [SerializeField] private Texture2D unauthorizedTexture;
    [SerializeField] private Button authorizeButton;

    private void Awake()
    {
        UpdateData();
    }

    private void OnEnable()
    {
        YG2.onGetSDKData += UpdateData;
        authorizeButton.onClick.AddListener(YG2.OpenAuthDialog);
    }

    private void OnDisable()
    {
        YG2.onGetSDKData -= UpdateData;
        authorizeButton.onClick.RemoveListener(YG2.OpenAuthDialog);
    }

    private void UpdateData()
    {
        UpdateName();
        UpdateImage();
        UpdateId();
    }

    private void UpdateName()
    {
        playerName.text = YG2.player.name;
    }

    private void UpdateImage()
    {
        if (playerImage != null && YG2.player.auth)
        {
            playerImage.Load(YG2.player.photo);
        }
        else if (unauthorizedTexture)
        {
            playerImage.SetTexture(unauthorizedTexture);
        }
        playerImage.spriteImage.preserveAspect = true;
    }

    private void UpdateId()
    {
        playerId.text = YG2.player.id;
    }
}
