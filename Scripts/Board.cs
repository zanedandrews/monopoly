using UnityEngine;

public enum eCam { start, mid }
public class Board : MonoBehaviour
{
    [NamedArray(typeof(ePos))] public Spot[] spots;
    [NamedArray(typeof(eCam))] public Transform[] camPt;
    [NamedArray(typeof(eCam))] public float[] fov = {44f, 20f};

    public Transform[] dieStartPts;
    public Dice[] die;
    public AudioClip audioRollDice;

    bool isTargeted;
    Transform target;    

    public void Start()
    {
        die = new Dice[dieStartPts.Length];
        MoveCamera(eCam.start, null);        
    }

    public void MoveCamera(eCam _cam, Transform _target)
    {
        target = _target;
        isTargeted = (target != null);
        Camera.main.transform.position = camPt[(int)_cam].position;
        Camera.main.transform.rotation = camPt[(int)_cam].rotation;
        Camera.main.fieldOfView = fov[(int)_cam];
    }
    public void RollDice()
    {
        DestroyDice();
        for (int i = 0; i < dieStartPts.Length; i++)
        {
            die[i] = Instantiate(Resources.Load("GameObjects/Board/" + "pDie") as GameObject, dieStartPts[i].position, Random.rotation).GetComponent<Dice>();            
        }
        GameManager.gm.audioManager.PlaySound(audioRollDice);
    }

    public void DestroyDice()
    {
        foreach (Dice d in die)
        {
            if (d != null) Destroy(d.gameObject);
            
        }
    }
   
    public void LateUpdate()
    {
        if (isTargeted)
        {
            Camera.main.transform.LookAt(target);
        }
    }
}
