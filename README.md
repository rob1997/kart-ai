# Kart AI
[![GitHub release](https://img.shields.io/github/v/release/rob1997/TrackGenerator?include_prereleases)](https://rob1997.github.io/kart-ai/)
[![Made with Unity](https://img.shields.io/badge/Made%20with-Unity-57b9d3.svg?style=flat&logo=unity)](https://unity3d.com)
[![GitHub license](https://img.shields.io/github/license/rob1997/TrackGenerator)](https://opensource.org/licenses/MIT)

A Game AI implementation using Reinforcement Learning via [ML-Agents]() in Unity.

Unity Version: `6000.0.31f1`

![demo](./demo.gif)

## Usage (Training)

You can open up `Heuristic` scene under `Assets/Scenes` to test the AI. However, if you'd like to train your own AI you can follow the steps below.

### 1. Set up your own Agent
To do this you can look at `Jeep.prefab` under `Assets/Prefabs`, you'll need to set up your Agent in a similar way (hierarchy matters). Once you've your Agent set up, you can take a look at `PlayerJeep.prefab` and `AiJeep_Training.prefab` in the same directory to set up prefab variants for Player and AI respectively.

### 2. Set up your Environment

To do this you can look at `Training` scene under `Assets/Scenes`, you'll need to set up your Environments in a similar way. Each Environment has a `Simulation` script and a `RandomTrackGenerator` script attached to it, moreover it has a plane with a `MeshCollider` and a trainable Agent as child objects. The number of Environments you have will determine the number of Agents you can train simultaneously.

### 3. Set up your Training Config File

You can find the training config file under `configs/ppo/kart-ai.yaml`. You can change the values to suit your needs based on the [ML-Agents documentation](https://unity-technologies.github.io/ml-agents/Training-Configuration-File/).

### 4. Train your AI

If you don't already have ML-Agents installed you can do so by following the instructions [here](https://unity-technologies.github.io/ml-agents/Installation/). Once you've ML-Agents installed you can train your AI by running the following command in the terminal.

```bash
mlagents-learn <path-to-config-file> --run-id=<run-identifier>
```

You can also find more information on training your Agent [here](https://unity-technologies.github.io/ml-agents/Training-ML-Agents/).

## Configuration

Every configurable property for the Environment, Agent and Training has a tooltip you can access by hovering over it on the inspector.

## How it Works

If you would like to know how it works, I've a dev-log entry on it [here](https://rob1997.github.io/devlog/log-4.html)

## Contributing

If you'd like to contribute to the project, you can fork the repository and create a pull request. You can also create an issue if you find any bugs or have any feature requests.