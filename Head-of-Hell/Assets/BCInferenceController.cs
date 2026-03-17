using System;
using Unity.Barracuda;
using UnityEngine;

[RequireComponent(typeof(CharacterManager))]
public class BCInferenceController : MonoBehaviour
{
    [Header("Bindings")]
    public CharacterManager selfManager;
    public CharacterManager enemyManager;

    [Header("Model")]
    public NNModel onnxModelAsset;

    [Header("Inference")]
    public string playerSuffix = "_P2";
    public int decisionInterval = 1;

    [Header("Observation Settings")]
    [SerializeField] private float relXScale = 9f;
    [SerializeField] private float relYScale = 5f;
    [SerializeField] private float velScale = 10f;
    [SerializeField] private int totalCharacterCount = 10;

    private Character self;
    private Character opp;

    private AIInputProvider aiInput;

    private Model runtimeModel;
    private IWorker worker;

    private int frameCounter = 0;

    // cached keys like FighterAgent
    KeyCode upK, downK, leftK, rightK, lightK, heavyK, blockK, abilityK, chargeK, parryK;

    private void Start()
    {
        if (selfManager == null)
            selfManager = GetComponent<CharacterManager>();

        if (selfManager != null)
        {
            selfManager.OnCharacterReady += BindSelf;
            selfManager.OnCharacterChanged += BindSelf;
        }

        if (enemyManager != null)
        {
            enemyManager.OnCharacterReady += BindEnemy;
            enemyManager.OnCharacterChanged += BindEnemy;
        }

        TryBindNow();

        if (onnxModelAsset == null)
        {
            Debug.LogError("[BCInferenceController] No ONNX model assigned.");
            enabled = false;
            return;
        }

        runtimeModel = ModelLoader.Load(onnxModelAsset);
        worker = WorkerFactory.CreateWorker(WorkerFactory.Type.Auto, runtimeModel);
    }

    private void Update()
    {
        if (worker == null)
            return;

        if (self == null || opp == null)
        {
            TryBindNow();
            return;
        }

        frameCounter++;
        if (frameCounter % decisionInterval != 0)
            return;

        float[] obs = BCObservationEncoder.Encode(
            self,
            opp,
            relXScale,
            relYScale,
            velScale,
            totalCharacterCount
        );

        if (obs.Length != 64)
        {
            Debug.LogError($"[BCInferenceController] Expected 64 obs, got {obs.Length}");
            return;
        }

        using Tensor inputTensor = new Tensor(1, obs.Length);
        for (int i = 0; i < obs.Length; i++)
            inputTensor[0, i] = obs[i];

        worker.Execute(inputTensor);

        int move   = ArgMax(worker.PeekOutput("move_logits"));
        int jump   = ArgMax(worker.PeekOutput("jump_logits"));
        int drop   = ArgMax(worker.PeekOutput("drop_logits"));
        int light  = ArgMax(worker.PeekOutput("light_logits"));
        int heavy  = ArgMax(worker.PeekOutput("heavy_logits"));
        int block  = ArgMax(worker.PeekOutput("block_logits"));
        int special= ArgMax(worker.PeekOutput("special_logits"));
        int charge = ArgMax(worker.PeekOutput("charge_logits"));
        int parry  = ArgMax(worker.PeekOutput("parry_logits"));

        Debug.Log(
            $"BC obs: relX={obs[0]:F3}, relY={obs[1]:F3} | " +
            $"move={move}, jump={jump}, drop={drop}, light={light}, heavy={heavy}, " +
            $"block={block}, special={special}, charge={charge}, parry={parry}"
        );

        var cmd = new AIInputProvider.Command
        {
            moveX = move == 0 ? -1 : (move == 1 ? 0 : 1),
            jump = (jump == 1),
            drop = (drop == 1),
            light = (light == 1),
            heavy = (heavy == 1),
            blockHold = (block == 1),
            special = (special == 1),
            chargeHold = (charge == 1),
            chargeRelease = (charge == 2),
            parry = (parry == 1)
        };

        aiInput?.Apply(cmd);

        Tensor moveOut = worker.PeekOutput("move_logits");
        Debug.Log($"move logits: [{moveOut[0]:F3}, {moveOut[1]:F3}, {moveOut[2]:F3}]");
    }

    private int ArgMax(Tensor t)
    {
        int best = 0;
        float bestVal = t[0];

        for (int i = 1; i < t.length; i++)
        {
            if (t[i] > bestVal)
            {
                bestVal = t[i];
                best = i;
            }
        }

        return best;
    }

    private void TryBindNow()
    {
        if (self == null && selfManager != null)
        {
            var c = selfManager.CharacterChoice(1);
            if (c != null)
                BindSelf(c);
        }

        if (opp == null && enemyManager != null)
        {
            var e = enemyManager.CharacterChoice(1);
            if (e != null)
                BindEnemy(e);
        }
    }

    private void BindSelf(Character c)
    {
        self = c;
        if (self == null) return;

        var setup = self.GetComponent<CharacterSetup>();

        upK = setup.up;
        downK = setup.down;
        leftK = setup.left;
        rightK = setup.right;
        lightK = setup.lightAttack;
        heavyK = setup.heavyAttack;
        blockK = setup.block;
        abilityK = setup.ability;
        chargeK = setup.charge;
        parryK = setup.parry;

        if (aiInput == null)
            aiInput = new AIInputProvider(playerSuffix);

        aiInput.SetKeys(leftK, rightK, upK, downK, lightK, heavyK, blockK, abilityK, chargeK, parryK);
        self.SetInput(aiInput);

        Debug.Log($"[BCInferenceController] Bound SELF: {self.name}");
    }

    private void BindEnemy(Character c)
    {
        opp = c;
        if (opp != null)
            Debug.Log($"[BCInferenceController] Bound OPP: {opp.name}");
    }

    private void OnDestroy()
    {
        if (selfManager != null)
        {
            selfManager.OnCharacterReady -= BindSelf;
            selfManager.OnCharacterChanged -= BindSelf;
        }

        if (enemyManager != null)
        {
            enemyManager.OnCharacterReady -= BindEnemy;
            enemyManager.OnCharacterChanged -= BindEnemy;
        }

        worker?.Dispose();
    }
}