using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;
using InvManager;

namespace Effects
{
    public enum EffectSource
    {
        none = 0,
        item = 1
    }

    public enum EffectType
    {
        defense = 0,
        regeneration = 1
    }
    class Effect
    {
        public EffectType type;
        public int strength;
        public List<int> parameters;
        public EffectSource sourceType = 0;
        public uint sourceID = 0;
        public float timer = 0f;
        public bool haveTimer = false;
        
        public Effect(EffectType effectType_, int strength_, EffectSource sourceType_ = EffectSource.none, uint sourceID_ = 0, float timer_ = 0f)
        {
            this.type = effectType_;
            this.strength = strength_;
            this.sourceType = sourceType_;
            this.sourceID = sourceID_;
            if (timer_ != 0f)
            {
                this.haveTimer = true;
                this.timer = timer_;
            }
        }

        public Item getSourceItem()
        {
            return Item.getItem(this.sourceID);
        }
        
        public string stringifyType()
        {
            return stringifyType(this.type);
        }

        public string stringifyParameters()
        {
            return stringifyParameters(this.parameters);
        }
        public static string stringifyType(EffectType type)
        {
            return type.ToString();
        }

        public static string stringifyParameters(List<int> parameters)
        {
            string finalString = "[";
            if (parameters == null)
            {
                return "no parameters";
            }
            foreach (int parameter in parameters)
            {
                finalString += parameter.ToString() + ", ";
            }
            
            if (parameters.Count == 0)
            {
                finalString = "no parameters";
            }
            else
            {
                finalString = finalString.Remove(finalString.Length - 2, 2);
                finalString += "]";
            }
            
            return finalString;
        }
    }
	class Effects
    {
        public List<Effect> effects = new List<Effect>();

        public List<Effect> effectsCalced = new List<Effect>();

        public void updateTimers(float delta)
        {
            List<Effect> removeList = new List<Effect>();
            foreach (Effect effect in effects)
            {
                if (effect.haveTimer)
                {
                    effect.timer -= delta;
                    if (effect.timer <= 0)
                    {
                        removeList.Add(effect);
                    }
                }
            }

            foreach (Effect effect in removeList)
            {
                effects.Remove(effect);
            }

            if (removeList.Count > 0)
            {
                calcEffects();
            }
        }
        
        public void calcEffects()
        {
            this.effectsCalced.Clear();
            foreach (Effect e in effects)
            {
                bool found = false;
                foreach (Effect e2 in this.effectsCalced)
                {
                    if (e.type == e2.type)
                    {
                        e2.strength += e.strength;
                        found = true;
                    }
                }

                if (!found)
                {
                    this.effectsCalced.Add(new Effect(e.type, e.strength, e.sourceType, e.sourceID));
                }
            }
        }
        
        public Effect getCalcedEffect(EffectType type)
        {
            foreach (Effect effect in effectsCalced)
            {
                if (effect.type == type)
                {
                    return effect;
                }
            }
            return null;
        }

        public Effect getEffect(EffectType type)
        {
            foreach (Effect effect in effects)
            {
                if (effect.type == type)
                {
                    return effect;
                }
            }
            return null;
        }

        public Effect getEffectBySource(EffectSource effectSource, uint sourceID, EffectType? type = null)
        {
            foreach (Effect effect in effects)
            {
                if (effect.sourceType == effectSource && effect.sourceID == sourceID && (type == null || effect.type == type.Value))
                {
                    return effect;
                }
            }
            return null;
        }
        
        public void addEffect(Effect effect)
        {
            this.effects.Add(effect);
            this.calcEffects();
        }

        public Effect addEffect(EffectType type, int strength, EffectSource sourceType = EffectSource.none, uint sourceID = 0, float timer = 0f)
        {
            Effect effect = new Effect(type, strength, sourceType, sourceID, timer);
            this.addEffect(effect);
            return effect;
        }

        public void clear()
        {
            this.effects.Clear();
            this.effectsCalced.Clear();
        }

        public void clearItemsEffects()
        {
            List<Effect> removeList = new List<Effect>();
            foreach (Effect e in this.effects)
            {
                if (e.sourceType == EffectSource.item)
                {
                    removeList.Add(e);
                }
            }

            foreach (Effect e in removeList)
            {
                this.effects.Remove(e);
            }

            removeList.Clear();
            foreach (Effect e in this.effectsCalced)
            {
                if (e.sourceType == EffectSource.item)
                {
                    removeList.Add(e);
                }
            }

            foreach (Effect e in removeList)
            {
                this.effectsCalced.Remove(e);
            }
            removeList.Clear();
        }

        public void removeEffect(Effect effect)
        {
            foreach (Effect e in this.effects)
            {
                if (e.type == effect.type)
                {
                    effects.Remove(e);
                    this.calcEffects();
                    return;
                }
            }
        }
        
        public void removeEffectBySource(EffectSource effectSource, uint sourceID, EffectType? type = null)
        {
            foreach (Effect e in this.effects)
            {
                if (e.sourceType == effectSource && e.sourceID == sourceID && (type == null || e.type == type))
                {
                    this.effects.Remove(e);
                    break;
                }
            }
        }
        
        public void removeEffect(EffectType type)
        {
            List<Effect> removeList = new List<Effect>();
            foreach (Effect e in this.effects)
            {
                if (e.type == type)
                {
                    removeList.Add(e);
                }
            }

            foreach (Effect e in removeList)
            {
                this.effects.Remove(e);
            }
            removeList.Clear();
            
            this.calcEffects();
        }
        public void print()
        {
            foreach (Effect e in this.effects)
            {
                if (e.haveTimer)
                {
                    Debug.Log(e.stringifyType() + ": " + e.strength.ToString() + " (" + e.timer.ToString() + " sec.) " + e.stringifyParameters() + " | " + e.sourceType.ToString() + ":" + e.sourceID.ToString());
                }
                else
                {
                    Debug.Log(e.stringifyType() + ": " + e.strength.ToString() + " " + e.stringifyParameters() + " | " + e.sourceType.ToString() + ":" + e.sourceID.ToString());   
                }
            }
        }

        public void printCalced()
        {
            this.calcEffects();
            foreach (Effect e in this.effectsCalced)
            {
                int calcCombines = 0;
                foreach (Effect e2 in this.effects)
                {
                    if (e2.type == e.type)
                    {
                        calcCombines++;
                    }
                }
                Debug.Log(e.stringifyType() + ": " + e.strength.ToString() + " " + e.stringifyParameters() + " | combines: " + calcCombines.ToString());
            }
        }
    }
}