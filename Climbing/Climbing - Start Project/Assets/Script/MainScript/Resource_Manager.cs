using System.Collections;
using System.Collections.Generic;
using UnityEditorInternal;
using UnityEngine;

public class Resource_Manager : MonoBehaviour
{
    public List<CharacterModels> characterModels = new List<CharacterModels>();

    public void SwitchCharacterModelWithIndex(StateManagerShoot st,int target)
    {
        StartCoroutine(SwitchCharacter(st, target));
    }

    IEnumerator SwitchCharacter(StateManagerShoot st, int target)
    {
        yield return SwitchCharacterWith(st, 0);
        yield return SwitchCharacterWith(st, target);
    }

    IEnumerator SwitchCharacterWith(StateManagerShoot st, int target)
    {
        if (!st.model.activeInHierarchy)
            st.model.SetActive(true);

        List<Sharable_Obj> getAllObjs = st.weaponManager.PopulateAndReturnSharableList();

        List<SharableAssetsInfo> l = new List<SharableAssetsInfo>();

        foreach(Sharable_Obj o in getAllObjs)
        {
            SharableAssetsInfo n = new SharableAssetsInfo();
            n.obj = o.gameObject;
            n.pos = o.transform.localPosition;
            n.rot = o.transform.localRotation;
            n.scale = o.transform.localScale;
            n.parentBone = o.parentBone;
            n.wasActive = n.obj.activeInHierarchy;
            n.obj.SetActive(false);
            n.obj.transform.parent = null;
            l.Add(n);
        }

        GameObject newModel = Instantiate(characterModels[target].prefab, Vector3.zero, Quaternion.identity) as GameObject;

        newModel.transform.parent = st.transform;
        newModel.transform.localPosition = Vector3.zero;
        newModel.transform.localRotation = Quaternion.Euler(0, 0, 0);

        if (target == 0)
            newModel.SetActive(false);

        GameObject prevModel = st.model;
        st.model = newModel;
        st.handleAnimations.SetupAnimator(newModel.GetComponent<Animator>());

        Destroy(prevModel);

        st.handleAnimations.anim.Rebind();

        for(int i = 0; i < l.Count; i++)
        {
            Transform t = l[i].obj.transform;

            t.parent = st.handleAnimations.anim.GetBoneTransform(l[i].parentBone);
            t.localPosition = l[i].pos;
            t.localRotation = l[i].rot;
            t.localScale = l[i].scale;

            if (l[i].wasActive)
            {
                l[i].obj.SetActive(true);
            }
        }

        yield return null;
    }


    int ReturnCharacterModelIndexFromID(string id)
    {
        int retVal = 0;

        for(int i = 0; i < characterModels.Count; i++)
        {
            if (string.Equals(characterModels[i].id, id))
            {
                retVal = i;
                break;
            }
        }
        return retVal;
    }

    public static Resource_Manager instance;
    public static Resource_Manager getInstance()
    {
        return instance;
    }

    private void Awake()
    {
        instance = this;
    }
}

[System.Serializable]
public class CharacterModels
{
    public string id;
    public GameObject prefab;
}
[System.Serializable]
public class SharableAssetsInfo
{
    public GameObject obj;
    public Vector3 pos;
    public Quaternion rot;
    public Vector3 scale;
    public bool wasActive;
    public HumanBodyBones parentBone;
}
