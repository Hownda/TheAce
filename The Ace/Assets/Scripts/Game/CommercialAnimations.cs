using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CommercialAnimations : MonoBehaviour
{
    public GameObject commercialPrefab;
    public Transform commercialParent;
    private Animator animator;
    private AnimationClip slide;

    private GameObject currentCommercial;
    public bool spawned = false;
    public float disappearanceDistance;

    public static CommercialAnimations instance;

    private void Awake()
    {
        instance = this;
        animator = GetComponentInChildren<Animator>();
        
    }
    // Start is called before the first frame update
    void Start()
    {
        currentCommercial = animator.gameObject;
    }

    // Update is called once per frame
    void Update()
    {
        if (currentCommercial == null)
        {
            spawned = false;
            currentCommercial = GetComponentInChildren<Animator>().gameObject;
        }

        if (currentCommercial.GetComponent<RectTransform>().anchoredPosition.x > disappearanceDistance && spawned == false)
        {
            
            GameObject newCommercial = Instantiate(commercialPrefab, commercialParent);
            Vector3 spawnPosition = new Vector3(-12.5f, 0, 0);
            newCommercial.GetComponentInChildren<Animator>().transform.localPosition = spawnPosition;
            spawned = true;
        }
    }
}
