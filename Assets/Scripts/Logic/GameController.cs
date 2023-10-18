using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour
{
    public static GameController instance;
    [SerializeField] private ObjectPoolManager poolManager;
    [SerializeField] private List<GameObject> spiders;
    [SerializeField] private MapController mapController;

    private int totalTypeSpiders;
    private void Awake()
    {
        instance = this;
        CacheSpiders();
    }
    
    void CacheSpiders()
    {
        if (poolManager != null)
        {
            if (spiders != null && spiders.Count > 0) {
                for (int i = 0; i < spiders.Count; i++)
                {
                    poolManager.PreCachePool(spiders[i], 20);
                }
            }
           
            totalTypeSpiders = spiders.Count;
        }
    }

    public void SpawnSpiders(int count)
    {
        if (poolManager != null && count > 0)
        {
            for (int i = 0; i < count; i++)
            {
                int type = Random.Range(0, totalTypeSpiders);
                var spider = poolManager._Spawn(spiders[type], Vector3.zero, Quaternion.identity, poolManager.transform);
                var spiderController = spider.GetComponent<MoveObj>();
                int gate = Random.Range(0, 3);
                switch (gate) {
                    case 1:
                        spiderController.SetPath(mapController.Path2);
                        break;
                    case 2:
                        spiderController.SetPath(mapController.Path3);
                        break;
                    case 0:
                    default:
                        spiderController.SetPath(mapController.Path1);
                        break;
                }
                spider.SetActive(true);
            }
        }
    }
}
