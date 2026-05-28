using Assets.Scripts.DeathConsequencesSystems;
using System;
using Unity.Entities;
using UnityEngine;
using YG; // PluginYG2 пространство имен

public class RewardedAdvController : MonoBehaviour
{
    public static RewardedAdvController Instance { get; private set; }

    private Action currentErrorCallback;

    private void Awake()
    {
        if (Instance != null)
        {
            Debug.LogWarning("RewardedAdvController already exists, destroying duplicate");
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        YG2.onErrorRewardedAdv += OnRewardedAdvError;
    }

    private void OnDestroy()
    {
        YG2.onErrorRewardedAdv -= OnRewardedAdvError;
    }

    public void ShowRewardedAdv(RewardedAdvAwards rewardId, Action onSuccess = null, Action onError = null)
    {
        currentErrorCallback = onError;

        if (YG2.nowRewardAdv)
        {
            return;
        }

        var world = World.DefaultGameObjectInjectionWorld;

        switch (rewardId)
        {
            case RewardedAdvAwards.extraLife:
                YG2.RewardedAdvShow(rewardId.ToString(), () =>
                {
                    if (world == null || !world.IsCreated)
                        return;

                    var playerQuery = world.EntityManager.CreateEntityQuery(
                        typeof(CharacterCurrentHitPoints),
                        typeof(CharacterMaxHitPoints),
                        typeof(PlayerTag));

                    if (!playerQuery.IsEmpty)
                    {
                        var playerEntity = playerQuery.GetSingletonEntity();
                        var maxHitPoints = world.EntityManager.GetComponentData<CharacterMaxHitPoints>(playerEntity);

                        world.EntityManager.SetComponentData(playerEntity, new CharacterCurrentHitPoints
                        {
                            Value = maxHitPoints.Value
                        });
                        world.EntityManager.SetComponentEnabled<DeathEntityFlag>(playerEntity, false);
                        EnemyPoolUtility.ReturnAllActiveEnemiesToPool(world.EntityManager);

                        Debug.Log("End Revive");
                    }

                    playerQuery.Dispose();
                    GameUIController.Instance.SwitchDeathPanel();
                });
                break;
            case RewardedAdvAwards.chest:
                YG2.RewardedAdvShow(rewardId.ToString(), () =>
                {
                    onSuccess?.Invoke();
                });
                break;
            case RewardedAdvAwards.gold:
                YG2.RewardedAdvShow(rewardId.ToString(), () =>
                {
                    onSuccess?.Invoke();
                });
                break;
            case RewardedAdvAwards.gems:
                YG2.RewardedAdvShow(rewardId.ToString(), () =>
                {
                    onSuccess?.Invoke();
                });
                break;
        }
    }

    private void OnRewardedAdvError()
    {
        currentErrorCallback?.Invoke();

        ClearCurrentCallbacks();
    }

    private void ClearCurrentCallbacks()
    {
        currentErrorCallback = null;
    }
}
