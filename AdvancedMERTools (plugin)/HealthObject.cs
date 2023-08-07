﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Exiled.API.Features.Items;
using InventorySystem.Items.Armor;
using Exiled.API.Features;

namespace AdvancedMERTools
{
    public class HealthObject : MonoBehaviour
    {
        private void Start()
        {
            Health = Base.Health;
            AdvancedMERTools.Singleton.healthObjects.Add(this);
        }

        public void OnShot(Exiled.Events.EventArgs.Player.ShotEventArgs ev)
        {
            if (!ev.CanHurt || ev.Player == null || !IsAlive) return;
            Firearm firearm = ev.Player.CurrentItem as Firearm;
            float damage = BodyArmorUtils.ProcessDamage(Base.ArmorEfficient, firearm.Base.BaseStats.DamageAtDistance(firearm.Base, ev.Distance), Mathf.RoundToInt(firearm.Base.ArmorPenetration * 100f));
            Health -= damage;
            CheckDead(ev.Player);
        }

        public void OnGrenadeExplode(Exiled.Events.EventArgs.Map.ExplodingGrenadeEventArgs ev)
        {
            if (!ev.IsAllowed || !IsAlive) return;
            if (ev.Projectile.Type == ItemType.GrenadeHE)
            {
                float Dis = Vector3.Distance(this.transform.position, ev.Position);
                float damage = Dis < 4 ? 300f + Mathf.Max(0f, 30f * (Mathf.Pow(Dis, 2f) - Mathf.Pow(2f, Dis))) : 2160 / Dis - 240f;
                Health -= damage;
                CheckDead(ev.Player);
            }
        }

        public void CheckDead(Player player)
        {
            if (Health <= 0)
            {
                HealthObjectDTO clone = new HealthObjectDTO
                {
                    Health = this.Base.Health,
                    ArmorEfficient = this.Base.ArmorEfficient,
                    DeadType = this.Base.DeadType,
                    ObjectId = this.Base.ObjectId
                };
                IsAlive = false;
                EventHandler.OnHealthObjectDead(new HealthObjectDeadEventArgs(clone, player));
                switch (Base.DeadType)
                {
                    case DeadType.Disappear:
                        Destroy();
                        break;
                    case DeadType.GetRigidbody:
                        this.gameObject.AddComponent<Rigidbody>();
                        break;
                    case DeadType.DynamicDisappearing:
                        break;
                    case DeadType.Explode:
                        Utils.ExplosionUtils.ServerExplode(this.transform.position, player.Footprint);
                        Destroy();
                        break;
                    case DeadType.ResetHP:
                        Health = Base.Health;
                        IsAlive = true;
                        break;
                }
            }
        }

        void Update()
        {
            if (Health <= 0 && this.Base.DeadType == DeadType.DynamicDisappearing)
            {
                this.transform.localScale = Vector3.Lerp(this.transform.localScale, Vector3.zero, Time.deltaTime);
                if (this.transform.localScale.magnitude <= 0.1f)
                {
                    Destroy();
                }
            }
        }

        void Destroy()
        {
            AdvancedMERTools.Singleton.healthObjects.Remove(this);
            Destroy(this.gameObject);
        }

        public float Health;

        public bool IsAlive = true;

        public HealthObjectDTO Base;
    }
}
