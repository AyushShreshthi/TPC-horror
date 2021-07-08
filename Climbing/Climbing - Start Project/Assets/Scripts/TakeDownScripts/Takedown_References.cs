using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Takedown_References : MonoBehaviour
{
    public Animator anim;

    public List<BoneList> boneList = new List<BoneList>();
    public List<RegularMesh> meshList = new List<RegularMesh>();

    public Material blackMaterial;

    public bool initOnStart;

    private void Start()
    {
        if (initOnStart)
        {
            Init();
        }
    }

    public void Init()
    {
        boneList.Clear();
        meshList.Clear();

        anim = GetComponent<Animator>();

        SkinnedMeshRenderer[] sR = transform.GetComponentsInChildren<SkinnedMeshRenderer>();

        foreach(SkinnedMeshRenderer s in sR)
        {
            string n = s.name;
            string[] section = n.Split(new string[] { "_" }, StringSplitOptions.RemoveEmptyEntries);

            if (section.Length == 2)
            {
                string p = section[0];
                string id = section[1];

                if (string.Equals(p, "Skl"))
                {
                    BoneList b = new BoneList();
                    b.bone = s.gameObject;
                    b.boneId = id;

                    boneList.Add(b);
                }
                else
                {
                    RegularMesh mesh = new RegularMesh();

                    mesh.ren = s;
                    mesh.mat = s.material;

                    meshList.Add(mesh);
                }
            }
            else
            {
                RegularMesh mesh = new RegularMesh();

                mesh.ren = s;
                mesh.mat = s.material;

                meshList.Add(mesh);
            }
        }

        CloseSkeleton();
    }
    public void OpenSkeleton()
    {
        foreach(BoneList b in boneList)
        {
            if (!b.destroyed)
                b.bone.SetActive(true);
        }

        ChangeMaterialsToBlack();
    }

    public void ChangeMaterialsToBlack()
    {
        foreach(RegularMesh m in meshList)
        {
            m.ren.material = blackMaterial;
        }
    }

    public void CloseSkeleton()
    {
        foreach(BoneList b in boneList)
        {
            b.bone.SetActive(false);
        }

        ReverseMaterials();
    }

    public void ReverseMaterials()
    {
        foreach(RegularMesh m in meshList)
        {
            m.ren.material = m.mat;
        }
    }

    public void ChangeLayer(int i)
    {
        foreach(RegularMesh m in meshList)
        {
            m.ren.gameObject.layer = i;
        }

        MeshRenderer[] storeMeshes = transform.GetComponentsInChildren<MeshRenderer>();

        foreach(MeshRenderer m in storeMeshes)
        {
            m.gameObject.layer = i;
        }
    }
}

[System.Serializable]
public class BoneList
{
    public string boneId;
    public GameObject bone;
    public bool destroyed;
}

[System.Serializable]
public class RegularMesh
{
    public SkinnedMeshRenderer ren;
    public Material mat;
}
