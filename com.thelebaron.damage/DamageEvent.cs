using System;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Damage
{
    public struct DamageEvent: IComponentData
    {
        public int Amount;
        public Entity Receiver;
        public Entity Sender;
    }
}