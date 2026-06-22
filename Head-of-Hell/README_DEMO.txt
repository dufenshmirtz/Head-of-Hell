RL Agent Thesis Demo
====================

What this demo shows
--------------------
This demo lets a human Player 1 play a normal one-round match against Player 2 controlled by the trained reinforcement learning agent. The model runs with Unity ML-Agents inference only.

The demo menu is in:
Assets/Scenes/RLAgentDemo.unity

The RLAgentDemoMenu component exposes Beginner, Intermediate, and Expert model fields. You can drag and drop replacement NNModel/ONNX assets into those fields for demo builds without changing the normal PvE gameplay scene setup.
The same component also exposes the menu and controls text fields, so the visible demo copy can be edited from the Inspector.

How to run the build
--------------------
For a thesis demo executable, build with RLAgentDemo as the first scene and GamePlayScene included after it. The scene has also been added to Build Settings without changing the normal game scene order, so the regular game flow is preserved.

After building, run the standalone executable normally. No Unity Editor, Python environment, mlagents-learn command, or ML-Agents training process is required at runtime.

How to play against the agent
-----------------------------
1. Launch the build.
2. Pick Agent 1 Level and Agent 2 Level with their on-screen arrows.
3. Pick the Player 1 character and the Agent character with the on-screen arrows.
4. Select "Play vs Final RL Agent" for human vs agent, or "Watch Agent vs Agent" to let both players run inference.
5. In human vs agent, Player 1 is human-controlled and Player 2 uses Agent 2 Level.
6. In agent vs agent, Player 1 uses Agent 1 Level and Player 2 uses Agent 2 Level.

Controls
--------
Move: A / D
Jump: W
Drop: S
Quick Attack: U
Heavy Attack: I
Block: O
Special: P
Charge: J
Pause: Esc
Quick restart during a match: Enter
Return to demo menu during a demo match: Backspace

Gameplay rules
--------------
The demo applies the default gameplay rules: 100 HP, one round, normal arena, normal controls, no training mode, and no external trainer.

Technical note
--------------
This demo allows the user to play against the final reinforcement learning agent developed for the thesis. The model was trained with PPO using Unity ML-Agents. The demo selects PvE ML Agent mode, fixes the bot to Player 2, and uses BehaviorParameters in InferenceOnly mode.
Agent-vs-agent mode can use different selected model slots for Player 1 and Player 2 and does not run training.

Disclaimer
--------------
As the game is still in development many animations are missing or are incomplete.