class WingAttach : MonoBehaviour
{
    public GameObject prefabLeft;
    public GameObject prefabRight;
    GameObject[] created;
    List<Transform> dummyWings = new List<Transform>();
    void Start()
    {
        Transform[] p = null;
        if (Identifiable.MEAT_CLASS.Contains(GetComponentInParent<Identifiable>()?.id ?? Identifiable.Id.NONE))
        {
            p = new[] { new GameObject("dummy_wing_bone_left").transform, new GameObject("dummy_wing_bone_right").transform };
            p[0].SetParent(transform.parent.parent.parent.Find("bone_spine/bone_l_wing1"), false);
            p[1].SetParent(transform.parent.parent.parent.Find("bone_spine/bone_r_wing1"), false);
            p[0].localPosition = new Vector3(0, 0.1f, -0.05f);
            p[1].localPosition = new Vector3(0, -0.1f, 0.05f);
            p[0].localRotation = Quaternion.Euler(20, 75, 80);
            p[1].localRotation = Quaternion.Euler(20, 75, -100);
            dummyWings.AddRange(p);
        }
        else
            p = new[] { transform.parent?.Find("bone_slime/bone_wing_left"), transform.parent?.Find("bone_slime/bone_wing_right") };
        if (Identifiable.IsGordo(GetComponentInParent<GordoIdentifiable>()?.id ?? Identifiable.Id.NONE))
        {
            if (p == null)
                p = new Transform[2];
            if (p.Length > 0 && !p[0]) {
                p[0] = new GameObject("dummy_wing_bone_left").transform;
                p[0].SetParent(transform.parent?.Find("bone_slime"),false);
                p[0].localPosition = new Vector3(-0.3f, 0.8f, -0.3f);
                p[0].localRotation = Quaternion.Euler(30, 45, 30);
                dummyWings.Add(p[0]);
            }
            if (p.Length > 1 && !p[1])
            {
                p[1] = new GameObject("dummy_wing_bone_right").transform;
                p[1].SetParent(transform.parent?.Find("bone_slime"), false);
                p[1].localPosition = new Vector3(0.3f, 0.8f, -0.3f);
                p[1].localRotation = Quaternion.Euler(-150, -45, -150);
                dummyWings.Add(p[1]);
            }
        }
        if (GetComponentInParent<Drone>())
        {
            if (p == null)
                p = new Transform[2];
            if (p.Length > 0 && !p[0])
            {
                p[0] = new GameObject("dummy_wing_bone_left").transform;
                p[0].SetParent(transform.parent?.Find("bone_slime"), false);
                p[0].localPosition = new Vector3(-0.4f, 0.4f, -0.2f);
                p[0].localRotation = Quaternion.Euler(30, 45, 30);
            }
            if (p.Length > 1 && !p[1])
            {
                p[1] = new GameObject("dummy_wing_bone_right").transform;
                p[1].SetParent(transform.parent?.Find("bone_slime"), false);
                p[1].localPosition = new Vector3(0.4f, 0.4f, -0.2f);
                p[1].localRotation = Quaternion.Euler(-150, -45, -150);
            }
        }
        if (p != null)
        {
            created = new GameObject[p.Length];
            for (int i = 0; i < p.Length; i++)
                if ((i % 2) == 0 ? prefabLeft : prefabRight)
                    created[i] = Instantiate((i % 2) == 0 ? prefabLeft : prefabRight, p[i]);
        }
    }
    void OnDestroy()
    {
        if (created != null)
            foreach (var o in created)
                Destroy(o);
        foreach (var o in dummyWings)
            Destroy(o.gameObject);
    }
}