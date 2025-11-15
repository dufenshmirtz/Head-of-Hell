using UnityEngine;
using Photon.Pun;

public class NetworkCharacterLoader : MonoBehaviour
{
    public int playerNum; // Set by spawner
    private PhotonView pv;
    private Animator animator;
    private SpriteRenderer sr;

    void Awake()
    {
        pv = GetComponent<PhotonView>();
        animator = GetComponent<Animator>();
        sr = GetComponent<SpriteRenderer>();
    }

    void Start()
    {
        if (!pv.IsMine)
            return;

        string chosen = (playerNum == 1) ?
            PlayerPrefs.GetString("Player1Choice") :
            PlayerPrefs.GetString("Player2Choice");

        if (chosen == "Random")
            chosen = PickRandom();

        LoadCharacter(chosen);

        Debug.Log($"[NET] Loaded character: {chosen} for Player {playerNum}");
    }


    // --- Runs a switch like your old CharacterManager ---
    void LoadCharacter(string name)
    {
        switch (name)
        {
            case "Fin":
                gameObject.AddComponent<Fin>();
                animator.runtimeAnimatorController = Resources.Load<RuntimeAnimatorController>("Animators/FinAnimator");
                sr.color = Color.white;
                break;

            case "Lupen":
                gameObject.AddComponent<Lupen>();
                animator.runtimeAnimatorController = Resources.Load<RuntimeAnimatorController>("Animators/LupenAnimator");
                sr.color = Color.green;
                break;

                // Add all characters here...
        }
    }

    string PickRandom()
    {
        string[] chars = {
            "Fin","Lupen","Rager","Skipler","Vander",
            "Steelager","Lazy Bigus","Lithra","Chiback","Visvia"
        };

        return chars[Random.Range(0, chars.Length)];
    }
}
