using UnityEngine;
using UnityEngine.Events;
using Ivayami.Save;
using Ivayami.Player;
using System.Linq;

namespace Ivayami.Puzzle
{
    public class PedestalGroup : Activable
    {
        [SerializeField] private bool _resetAllPedestalsOnFail = true;
        [SerializeField] private UnityEvent _onActivate;
        [SerializeField] private UnityEvent _onFailActivate;
        private Pedestal[] _pedestals;

        public Pedestal[] Pedestals => _pedestals;

        protected override void Awake()
        {
            base.Awake();
            _pedestals = new Pedestal[activators.Length];
            for (int i = 0; i < activators.Length; i++)
            {
                _pedestals[i] = activators[i].GetComponent<Pedestal>();
            }
        }

        protected override void HandleOnActivate()
        {
            base.HandleOnActivate();
            if (IsActive)
            {
                for (int i = 0; i < _pedestals.Length; i++)
                {
                    if (!_pedestals[i].DeliverUI.CheckRequestsCompletion())
                    {
                        if (_resetAllPedestalsOnFail) DeactivateAllPedestals();
                        _onFailActivate?.Invoke();
                        return;
                    }
                }
                _onActivate?.Invoke();
            }
        }

        private void DeactivateAllPedestals()
        {
            for (int i = 0; i < _pedestals.Length; i++)
            {
                _pedestals[i].DeactivatePedestal();
            }
        }

        public void LoadData(DeliverUISave.Data deliverUIData, PedestalGroupSave.Data data)
        {
            InventoryItem[] items = Resources.LoadAll<InventoryItem>("Items");
            for (int i = 0; i < data.PedestalDatas.Length; i++)
            {
                _pedestals[data.PedestalDatas[i].Index].LoadData(deliverUIData, items.First(x => x.name == data.PedestalDatas[i].ItemID));
            }
            Resources.UnloadUnusedAssets();
        }
    }
}