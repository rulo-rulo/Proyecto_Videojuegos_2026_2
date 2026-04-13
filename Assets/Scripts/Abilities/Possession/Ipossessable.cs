using UnityEngine;

namespace Possession
{
    public interface IPossessable
    {
        WeightClass WeightClass { get; }
        Transform Transform { get; }

        void OnPossess(float speed);
        void OnDepossess();
    }
}