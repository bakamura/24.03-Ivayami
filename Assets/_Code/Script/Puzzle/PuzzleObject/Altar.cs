using Ivayami.Audio;
using Ivayami.Player;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using static Ivayami.Puzzle.Lock;

namespace Ivayami.Puzzle
{
    [RequireComponent(typeof(InteractableFeedbacks), typeof(InteractableSounds))]
    public class Altar : MonoBehaviour, IInteractable
    {
        public InteractableFeedbacks InteratctableHighlight => throw new System.NotImplementedException();

        public PlayerActions.InteractAnimation Interact()
        {
            throw new System.NotImplementedException();
        }
    }
}