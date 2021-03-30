using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SlugBase;
using UnityEngine;
using RWCustom;
using System.IO;
using System.Reflection;


public class ElectricCat : SlugBaseCharacter
{
    public On.Player.hook_ctor playerHook;
    public On.Player.hook_Update updateHook;
    public On.PlayerGraphics.hook_InitiateSprites initHook;
    public On.PlayerGraphics.hook_DrawSprites drawHook;
    public On.PlayerGraphics.hook_ApplyPalette paletteHook;
    public On.PlayerGraphics.hook_AddToContainer containerHook;
    public On.PlayerGraphics.hook_Update pgUpdateHook;
    public On.Player.hook_Collide collideHook;
    public On.ElectricDeath.hook_Update edUpdateHook;
    public On.JellyFish.hook_BitByPlayer jellyBiteHook;
    public On.Centipede.hook_Shock centipedeShockHook;
    public On.ZapCoil.hook_Update zapCoilUpdateHook;
    public On.LanternMouse.hook_Carried lmCarryHook;
    public On.UnderwaterShock.hook_Update waterShockHook;
    public static List<ElectricVars> EVars;
    public ElectricCat() : base("Electric", FormatVersion.V1, 2)
    {
        playerHook = PlayerCtorHook;
        updateHook = PlayerUpdateHook;
        initHook = InitSpritesHook;
        drawHook = DrawSpritesHook;
        paletteHook = ApplyPaletteHook;
        containerHook = AddToContainerHook;
        pgUpdateHook = PlayerGraphicsUpdateHook;
        collideHook = PlayerCollideHook;
        edUpdateHook = ElectricDeathHook;
        jellyBiteHook = JellyFishBiteHook;
        centipedeShockHook = CentiShockHook;
        zapCoilUpdateHook = ZapUpdateHook;
        lmCarryHook = LanternMouseCarryHook;
        waterShockHook = UnderwaterShockHook;
    }

    //Name and Description
    public override string Description => "Danger, high voltage";
    public override string DisplayName => "The Electric Cat";

    //Tweaks
    public override bool HasDreams => false;
    public override bool HasGuideOverseer => false;

    //Resources
    public override Stream GetResource(params string[] path)
    {
        return Assembly.GetExecutingAssembly().GetManifestResourceStream("ElectricCat.Resources." + string.Join(".", path));
    }

    //Visuals
    public override Color? SlugcatColor()
    {
        return new Color(0.6509f, 0.3764f, 0f);
    }

    public override void GetFoodMeter(out int maxFood, out int foodToSleep)
    {
        maxFood = 8;
        foodToSleep = 4;
    }
    protected override void GetStats(SlugcatStats stats)
    {
        stats.throwingSkill = 2;
    }

    //Custom Hooks
    protected override void Disable()
    {
        On.Player.ctor -= playerHook;
        On.Player.Update -= updateHook;
        On.PlayerGraphics.InitiateSprites -= initHook;
        On.PlayerGraphics.DrawSprites -= drawHook;
        On.PlayerGraphics.ApplyPalette -= paletteHook;
        On.PlayerGraphics.AddToContainer -= containerHook;
        On.PlayerGraphics.Update -= pgUpdateHook;
        On.Player.Collide -= collideHook;
        //World
        On.ElectricDeath.Update -= edUpdateHook;
        On.JellyFish.BitByPlayer -= jellyBiteHook;
        On.Centipede.Shock -= centipedeShockHook;
        On.ZapCoil.Update -= zapCoilUpdateHook;
        On.LanternMouse.Carried -= lmCarryHook;
        On.UnderwaterShock.Update -= waterShockHook;
    }
    protected override void Enable()
    {
        //Player
        On.Player.ctor += playerHook;
        On.Player.Update += updateHook;
        On.PlayerGraphics.InitiateSprites += initHook;
        On.PlayerGraphics.DrawSprites += drawHook;
        On.PlayerGraphics.ApplyPalette += paletteHook;
        On.PlayerGraphics.AddToContainer += containerHook;
        On.PlayerGraphics.Update += pgUpdateHook;
        On.Player.Collide += collideHook;
        //World
        On.ElectricDeath.Update += edUpdateHook;
        On.JellyFish.BitByPlayer += jellyBiteHook;
        On.Centipede.Shock += centipedeShockHook;
        On.ZapCoil.Update += zapCoilUpdateHook;
        On.LanternMouse.Carried += lmCarryHook;
        On.UnderwaterShock.Update += waterShockHook;
    }

    private void UnderwaterShockHook(On.UnderwaterShock.orig_Update orig, UnderwaterShock self, bool eu)
    {
        self.evenUpdate = eu;
        float num = self.rad * (0.25f + 0.75f * Mathf.Sin(Mathf.InverseLerp(0f, (float)self.lifeTime, (float)self.frame) * 3.14159274f));
        for (int i = 0; i < self.room.abstractRoom.creatures.Count; i++)
        {
            if (self.room.abstractRoom.creatures[i].realizedCreature != null && self.room.abstractRoom.creatures[i].realizedCreature != self.expemtObject && self.room.abstractRoom.creatures[i].realizedCreature.Submersion > 0f)
            {
                if (self.room.abstractRoom.creatures[i].realizedCreature is Player && (self.room.abstractRoom.creatures[i].realizedCreature as Player).playerState.slugcatCharacter == PlayerManager.GetCustomPlayer("Electric").SlugcatIndex)
                {
                    break;
                }
                float num2 = 0f;
                for (int j = 0; j < self.room.abstractRoom.creatures[i].realizedCreature.bodyChunks.Length; j++)
                {
                    if (Custom.DistLess(self.pos, self.room.abstractRoom.creatures[i].realizedCreature.bodyChunks[j].pos, num))
                    {
                        num2 = Mathf.Max(num2, Custom.LerpMap(Vector2.Distance(self.pos, self.room.abstractRoom.creatures[i].realizedCreature.bodyChunks[j].pos), num / 2f, num, 1f, 0f, 0.5f) * self.room.abstractRoom.creatures[i].realizedCreature.bodyChunks[j].submersion);
                    }
                }
                if (num2 > 0f)
                {
                    for (int k = 0; k < self.room.abstractRoom.creatures[i].realizedCreature.bodyChunks.Length; k++)
                    {
                        self.room.abstractRoom.creatures[i].realizedCreature.bodyChunks[k].vel += Custom.RNV() * num2 * Mathf.Min(5f, self.room.abstractRoom.creatures[i].realizedCreature.bodyChunks[k].rad);
                    }
                    if (UnityEngine.Random.value < 0.25f)
                    {
                        self.room.AddObject(new UnderwaterShock.Flash(self.room.abstractRoom.creatures[i].realizedCreature.bodyChunks[UnityEngine.Random.Range(0, self.room.abstractRoom.creatures[i].realizedCreature.bodyChunks.Length)].pos, self.room.abstractRoom.creatures[i].realizedCreature.TotalMass * 60f * num2 + 140f, Mathf.Pow(num2, 0.2f), self.lifeTime - self.frame, self.color));
                    }
                    self.room.abstractRoom.creatures[i].realizedCreature.Violence(null, default(Vector2?), self.room.abstractRoom.creatures[i].realizedCreature.bodyChunks[UnityEngine.Random.Range(0, self.room.abstractRoom.creatures[i].realizedCreature.bodyChunks.Length)], null, Creature.DamageType.Electric, self.damage * num2, self.damage * num2 * 240f + 30f);
                }
            }
        }
        self.frame++;
        if (self.frame > self.lifeTime)
        {
            self.Destroy();
        }
    }

    private void LanternMouseCarryHook(On.LanternMouse.orig_Carried orig, LanternMouse self)
    {
        if (self.grabbedBy[0].grabber is Player && (self.grabbedBy[0].grabber as Player).playerState.slugcatCharacter == PlayerManager.GetCustomPlayer("Electric").SlugcatIndex)
        {
            if (!self.dead)
            {
                bool flag3 = self.room.aimap.TileAccessibleToCreature(self.mainBodyChunk.pos, self.Template) || self.room.aimap.TileAccessibleToCreature(self.bodyChunks[1].pos, self.Template);
                if (self.grabbedBy[0].grabber is Player && ((self.grabbedBy[0].grabber as Player).input[0].x != 0 || (self.grabbedBy[0].grabber as Player).input[0].y != 0))
                {
                    flag3 = false;
                }
                if (flag3)
                {
                    self.struggleCountdownA--;
                    if (self.struggleCountdownA < 0)
                    {
                        if (UnityEngine.Random.value < 0.008333334f)
                        {
                            self.struggleCountdownA = UnityEngine.Random.Range(20, 400);
                        }
                        for (int i = 0; i < 2; i++)
                        {
                            self.bodyChunks[i].vel += Custom.DegToVec(UnityEngine.Random.value * 360f) * 6f * UnityEngine.Random.value;
                        }
                    }
                }
                self.struggleCountdownB--;
                if (self.struggleCountdownB < 0 && UnityEngine.Random.value < 0.008333334f)
                {
                    self.struggleCountdownB = UnityEngine.Random.Range(10, 100);
                }
                if (!self.dead && self.graphicsModule != null && (self.struggleCountdownA < 0 || self.struggleCountdownB < 0))
                {
                    if (UnityEngine.Random.value < 0.025f)
                    {
                        (self.graphicsModule as MouseGraphics).ResetUnconsiousProfile();
                        self.Die();
                        for (int i = 0; i < EVars.Count; i++)
                        {
                            if (EVars[i].ply == self.grabbedBy[0].grabber as Player)
                            {
                                EVars[i].EnterChargedModeNoToll();
                            }
                        }
                    }
                    for (int j = 0; j < self.graphicsModule.bodyParts.Length; j++)
                    {
                        self.graphicsModule.bodyParts[j].pos += Custom.DegToVec(UnityEngine.Random.value * 360f) * 3f * UnityEngine.Random.value;
                        self.graphicsModule.bodyParts[j].vel += Custom.DegToVec(UnityEngine.Random.value * 360f) * 6f * UnityEngine.Random.value;
                    }
                }
            }
        }
        else
        {
            orig.Invoke(self);
        }
    }

    private void ZapUpdateHook(On.ZapCoil.orig_Update orig, ZapCoil self, bool eu)
    {
        self.soundLoop.Update();
        self.disruptedLoop.Update();
        if (self.turnedOn > 0.5f)
        {
            for (int i = 0; i < self.room.physicalObjects.Length; i++)
            {
                for (int j = 0; j < self.room.physicalObjects[i].Count; j++)
                {
                    for (int k = 0; k < self.room.physicalObjects[i][j].bodyChunks.Length; k++)
                    {
                        if ((self.horizontalAlignment && self.room.physicalObjects[i][j].bodyChunks[k].ContactPoint.y != 0) || (!self.horizontalAlignment && self.room.physicalObjects[i][j].bodyChunks[k].ContactPoint.x != 0))
                        {
                            Vector2 a = self.room.physicalObjects[i][j].bodyChunks[k].ContactPoint.ToVector2();
                            Vector2 v = self.room.physicalObjects[i][j].bodyChunks[k].pos + a * (self.room.physicalObjects[i][j].bodyChunks[k].rad + 30f);
                            if (self.GetFloatRect.Vector2Inside(v))
                            {
                                self.room.AddObject(new ZapCoil.ZapFlash(self.room.physicalObjects[i][j].bodyChunks[k].pos + a * self.room.physicalObjects[i][j].bodyChunks[k].rad, Mathf.InverseLerp(-0.05f, 15f, self.room.physicalObjects[i][j].bodyChunks[k].rad)));
                                self.room.physicalObjects[i][j].bodyChunks[k].vel -= (a * 6f + Custom.RNV() * UnityEngine.Random.value) / self.room.physicalObjects[i][j].bodyChunks[k].mass;
                                self.disruption = Mathf.Max(self.disruption, Mathf.InverseLerp(-0.05f, 9f, self.room.physicalObjects[i][j].bodyChunks[k].rad) + UnityEngine.Random.value * 0.5f);
                                self.smoothDisruption = self.disruption;
                                if (self.room.physicalObjects[i][j] is Creature)
                                {
                                    if (self.room.physicalObjects[i][j] is Player && (self.room.physicalObjects[i][j] as Player).playerState.slugcatCharacter == PlayerManager.GetCustomPlayer("Electric").SlugcatIndex)
                                    {
                                        for (int s = 0; s < EVars.Count; s++)
                                        {
                                            if (EVars[s].ply == (self.room.physicalObjects[i][j] as Player))
                                            {
                                                EVars[s].EnterChargedModeNoToll();
                                            }
                                        }
                                    }
                                    else
                                    {
                                        (self.room.physicalObjects[i][j] as Creature).Die();
                                    }
                                }
                                if (UnityEngine.Random.value < self.disruption && UnityEngine.Random.value < 0.5f)
                                {
                                    self.turnedOffCounter = UnityEngine.Random.Range(2, 15);
                                }
                                self.room.PlaySound(SoundID.Zapper_Zap, self.room.physicalObjects[i][j].bodyChunks[k].pos, 1f, 1f);
                                self.zapLit = 1f;
                            }
                        }
                    }
                }
            }
        }
        self.lastTurnedOn = self.turnedOn;
        if (UnityEngine.Random.value < 0.005f)
        {
            self.disruption = Mathf.Max(self.disruption, UnityEngine.Random.value);
        }
        self.disruption = Mathf.Max(0f, self.disruption - 1f / Mathf.Lerp(70f, 300f, UnityEngine.Random.value));
        self.smoothDisruption = Mathf.Lerp(self.smoothDisruption, self.disruption, 0.2f);
        float num = Mathf.InverseLerp(0.1f, 1f, self.smoothDisruption);
        self.soundLoop.Volume = (1f - num) * self.turnedOn;
        self.disruptedLoop.Volume = num * Mathf.Pow(self.turnedOn, 0.2f);
        for (int l = 0; l < self.flicker.GetLength(0); l++)
        {
            self.flicker[l, 1] = self.flicker[l, 0];
            self.flicker[l, 3] = Mathf.Clamp(self.flicker[l, 3] + Mathf.Lerp(-1f, 1f, UnityEngine.Random.value) / 10f, 0f, 1f);
            self.flicker[l, 2] += 1f / Mathf.Lerp(70f, 20f, self.flicker[l, 3]);
            self.flicker[l, 0] = Mathf.Clamp(0.5f + self.smoothDisruption * (Mathf.Lerp(0.2f, 0.1f, self.flicker[l, 3]) * Mathf.Sin(6.28318548f * self.flicker[l, 2]) + Mathf.Lerp(-1f, 1f, UnityEngine.Random.value) / 20f), 0f, 1f);
        }
        if (UnityEngine.Random.value < self.disruption && UnityEngine.Random.value < 0.0025f)
        {
            self.turnedOffCounter = UnityEngine.Random.Range(10, 100);
        }
        if (!self.powered)
        {
            self.turnedOn = Mathf.Max(0f, self.turnedOn - 0.1f);
        }
        if (self.turnedOffCounter > 0)
        {
            self.turnedOffCounter--;
            if (UnityEngine.Random.value < 0.5f || UnityEngine.Random.value > self.disruption || !self.powered)
            {
                self.turnedOn = 0f;
            }
            else
            {
                self.turnedOn = UnityEngine.Random.value;
            }
            if (self.powered)
            {
                self.turnedOn = Mathf.Lerp(self.turnedOn, 1f, self.zapLit * UnityEngine.Random.value);
            }
            self.smoothDisruption = 1f;
        }
        else if (self.powered)
        {
            self.turnedOn = Mathf.Min(self.turnedOn + UnityEngine.Random.value / 30f, 1f);
        }
        self.zapLit = Mathf.Max(0f, self.zapLit - 0.1f);
        if (self.room.fullyLoaded)
        {
            self.disruption = Mathf.Max(self.disruption, self.room.gravity);
        }
        if (self.room.roomSettings.GetEffectAmount(RoomSettings.RoomEffect.Type.BrokenZeroG) > 0f)
        {
            bool flag = self.room.world.rainCycle.brokenAntiGrav.to == 1f && self.room.world.rainCycle.brokenAntiGrav.progress == 1f;
            if (!flag)
            {
                self.disruption = 1f;
                if (self.powered && UnityEngine.Random.value < 0.2f)
                {
                    self.powered = false;
                }
            }
            if (flag && !self.powered && UnityEngine.Random.value < 0.025f)
            {
                self.powered = true;
            }
        }
    }

    private void CentiShockHook(On.Centipede.orig_Shock orig, Centipede self, PhysicalObject shockObj)
    {
        if (shockObj is Player && (shockObj as Player).playerState.slugcatCharacter == PlayerManager.GetCustomPlayer("Electric").SlugcatIndex)
        {
            if (self.graphicsModule != null)
            {
                (self.graphicsModule as CentipedeGraphics).lightFlash = 1f;
                for (int i = 0; i < (int)Mathf.Lerp(4f, 8f, self.size); i++)
                {
                    self.room.AddObject(new Spark(self.HeadChunk.pos, Custom.RNV() * Mathf.Lerp(4f, 14f, UnityEngine.Random.value), new Color(0.7f, 0.7f, 1f), null, 8, 14));
                }
            }
            for (int j = 0; j < self.bodyChunks.Length; j++)
            {
                self.bodyChunks[j].vel += Custom.RNV() * 6f * UnityEngine.Random.value;
                self.bodyChunks[j].pos += Custom.RNV() * 6f * UnityEngine.Random.value;
            }
            for (int k = 0; k < shockObj.bodyChunks.Length; k++)
            {
                shockObj.bodyChunks[k].vel += Custom.RNV() * 6f * UnityEngine.Random.value;
                shockObj.bodyChunks[k].pos += Custom.RNV() * 6f * UnityEngine.Random.value;
            }
            for (int i = 0; i < EVars.Count; i++)
            {
                if (EVars[i].ply == (shockObj as Player))
                {
                    if (self.Red)
                    {
                        EVars[i].EnterChargedModeNoToll();
                    }
                    else
                    {
                        EVars[i].EnterChargedModeNoToll();
                        self.Die();
                    }
                }
            }
        }
        else
        {
            orig.Invoke(self, shockObj);
        }
    }

    private void JellyFishBiteHook(On.JellyFish.orig_BitByPlayer orig, JellyFish self, Creature.Grasp grasp, bool eu)
    {
        if (grasp.grabber is Player && (grasp.grabber as Player).playerState.slugcatCharacter == PlayerManager.GetCustomPlayer("Electric").SlugcatIndex)
        {
            self.bites--;
            self.room.PlaySound((self.bites != 0) ? SoundID.Slugcat_Bite_Jelly_Fish : SoundID.Slugcat_Eat_Jelly_Fish, self.firstChunk.pos);
            self.firstChunk.MoveFromOutsideMyUpdate(eu, grasp.grabber.mainBodyChunk.pos);
            if (!self.AbstrConsumable.isConsumed)
            {
                self.AbstrConsumable.Consume();
            }
            for (int i = 0; i < self.tentacles.Length; i++)
            {
                for (int j = 0; j < self.tentacles[i].GetLength(0); j++)
                {
                    self.tentacles[i][j, 0] = Vector2.Lerp(self.tentacles[i][j, 0], self.firstChunk.pos, 0.2f);
                }
            }
            if (self.bites < 1)
            {
                for (int i = 0; i < EVars.Count; i++)
                {
                    if (EVars[i].ply == (grasp.grabber as Player))
                    {
                        EVars[i].EnterChargedModeNoToll();
                    }
                }
                //(grasp.grabber as Player).ObjectEaten(self);
                grasp.Release();
                self.Destroy();
            }
        }
        else
        {
            orig.Invoke(self, grasp, eu);
        }
    }

    private void ElectricDeathHook(On.ElectricDeath.orig_Update orig, ElectricDeath self, bool eu)
    {
        orig.Invoke(self, eu);
        if (self.Intensity > 0.5f && UnityEngine.Random.value < Custom.LerpMap(self.Intensity, 0.5f, 1f, 0f, 0.5f))
        {
            for (int j = 0; j < self.room.physicalObjects.Length; j++)
            {
                for (int k = 0; k < self.room.physicalObjects[j].Count; k++)
                {
                    for (int l = 0; l < self.room.physicalObjects[j][k].bodyChunks.Length; l++)
                    {
                        if (UnityEngine.Random.value < Custom.LerpMap(self.Intensity, 0.5f, 1f, 0f, 0.5f) && (self.room.physicalObjects[j][k].bodyChunks[l].ContactPoint.x != 0 || self.room.physicalObjects[j][k].bodyChunks[l].ContactPoint.y != 0 || self.room.GetTile(self.room.physicalObjects[j][k].bodyChunks[l].pos).AnyBeam))
                        {
                            float num2 = Mathf.Pow(UnityEngine.Random.value, 0.9f) * Mathf.InverseLerp(0.5f, 1f, self.Intensity);
                            self.room.AddObject(new ElectricDeath.SparkFlash(self.room.physicalObjects[j][k].bodyChunks[l].pos + self.room.physicalObjects[j][k].bodyChunks[l].rad * self.room.physicalObjects[j][k].bodyChunks[l].ContactPoint.ToVector2(), Mathf.Pow(num2, 0.5f)));
                            Vector2 vector = -(self.room.physicalObjects[j][k].bodyChunks[l].ContactPoint.ToVector2() + Custom.RNV()).normalized;
                            vector *= 22f * num2 / self.room.physicalObjects[j][k].bodyChunks[l].mass;
                            self.room.physicalObjects[j][k].bodyChunks[l].vel += vector;
                            self.room.physicalObjects[j][k].bodyChunks[l].pos += vector;
                            self.room.PlaySound(SoundID.Death_Lightning_Spark_Object, self.room.physicalObjects[j][k].bodyChunks[l].pos, num2, 1f);
                            if (self.room.physicalObjects[j][k] is Player && (self.room.physicalObjects[j][k] as Player).playerState.slugcatCharacter == PlayerManager.GetCustomPlayer("Electric").SlugcatIndex)
                            {
                                for (int i = 0; i < EVars.Count; i++)
                                {
                                    if (EVars[i].ply == (self.room.physicalObjects[j][k] as Player))
                                    {
                                        EVars[i].EnterChargedModeNoToll();
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    }

    private void PlayerCollideHook(On.Player.orig_Collide orig, Player self, PhysicalObject otherObject, int myChunk, int otherChunk)
    {
        orig.Invoke(self, otherObject, myChunk, otherChunk);
        if (self.playerState.slugcatCharacter == PlayerManager.GetCustomPlayer("Electric").SlugcatIndex)
        {
            for (int i = 0; i < EVars.Count; i++)
            {
                if (EVars[i].ply == self)
                {
                    if (otherObject is Creature && EVars[i].chargedActive && EVars[i].stunDelay == 0f)
                    {
                        if (!(otherObject is BigEel) && !(otherObject is Centipede) && !(otherObject is Fly) && !(otherObject as Creature).dead)
                        {
                            if (otherObject is Player && (otherObject as Player).playerState.slugcatCharacter == PlayerManager.GetCustomPlayer("Electric").SlugcatIndex)
                            {
                                return;
                            }
                            if (!(otherObject is Player) && (otherObject as Creature).abstractCreature.abstractAI.RealAI.friendTracker != null && (otherObject as Creature).abstractCreature.abstractAI.RealAI.friendTracker.Utility() > 0f)
                            {
                                return;
                            }
                            (otherObject as Creature).Violence(self.firstChunk, new Vector2?(Custom.DirVec(self.firstChunk.pos, otherObject.bodyChunks[otherChunk].pos) * 5f), otherObject.bodyChunks[otherChunk], null, Creature.DamageType.Electric, 0.8f, (!(otherObject is Player)) ? (270f * Mathf.Lerp((otherObject as Creature).Template.baseStunResistance, 1f, 0.5f)) : 140f);
                            self.room.AddObject(new CreatureSpasmer(otherObject as Creature, false, (otherObject as Creature).stun));
                            EVars[i].stunDelay = 300f;
                            self.room.PlaySound(SoundID.Jelly_Fish_Tentacle_Stun, self.firstChunk.pos);
                            self.room.AddObject(new Explosion.ExplosionLight(self.firstChunk.pos, 150f, 0.85f, 4, new Color(0.7f, 0.7f, 1f)));
                            EVars[i].chargedTimer -= 20f;
                        }
                    }
                }
            }
        }
    }

    private void PlayerGraphicsUpdateHook(On.PlayerGraphics.orig_Update orig, PlayerGraphics self)
    {
        if (self.player.playerState.slugcatCharacter == PlayerManager.GetCustomPlayer("Electric").SlugcatIndex)
        {
            for (int i = 0; i < EVars.Count; i++)
            {
                if (EVars[i].ply == self.player)
                {
                    if (self.lightSource != null)
                    {
                        self.lightSource.stayAlive = true;
                        self.lightSource.setPos = new Vector2?(self.player.firstChunk.pos);
                        self.lightSource.setRad = new float?(300f * Mathf.Pow(EVars[i].lightFlash * UnityEngine.Random.value, 0.01f) * Mathf.Lerp(0.5f, 2f, 0.8f) - 1.3f);
                        self.lightSource.setAlpha = new float?(Mathf.Pow(EVars[i].lightFlash * UnityEngine.Random.value, 0.01f) - 0.8f);
                        float num = EVars[i].lightFlash * UnityEngine.Random.value;
                        num = Mathf.Lerp(num, 1f, 0.5f * (1f - self.player.room.Darkness(self.player.firstChunk.pos)));
                        self.lightSource.color = new Color(num, num, 1.5f);
                        if (EVars[i].lightFlash <= 0f)
                        {
                            self.lightSource.Destroy();
                        }
                        if (self.lightSource.slatedForDeletetion)
                        {
                            self.lightSource = null;
                        }
                    }
                    else
                    {
                        if (EVars[i].lightFlash > 0f)
                        {
                            self.lightSource = new LightSource(self.player.firstChunk.pos, false, new Color(1f, 1f, 1f), self.player);
                            self.lightSource.affectedByPaletteDarkness = 0f;
                            self.lightSource.requireUpKeep = true;
                            self.player.room.AddObject(self.lightSource);
                        }
                    }
                    if (EVars[i].lightFlash > 0f)
                    {
                        EVars[i].lightFlash = Mathf.Max(0f, EVars[i].lightFlash - 0.0333933346f);
                    }
                    if (EVars[i].chargedActive)
                    {
                        for (int s = 0; s < (int)Mathf.Lerp(4f, 5f, 0.15f); s++)
                        {
                            Vector2 rng = new Vector2(0f, UnityEngine.Random.Range(-15f, 10f));
                            self.player.room.AddObject(new Spark(self.player.firstChunk.pos + rng, Custom.RNV() * Mathf.Lerp(4f, 14f, UnityEngine.Random.value), new Color(0.7f, 0.7f, 1f), null, 2, 14));
                        }
                    }
                }
            }
        }
        orig.Invoke(self);
    }

    private void AddToContainerHook(On.PlayerGraphics.orig_AddToContainer orig, PlayerGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
    {
        if (newContatiner == null)
        {
            newContatiner = rCam.ReturnFContainer("Midground");
        }
        for (int i = 0; i < sLeaser.sprites.Length; i++)
        {
            if ((i > 6 && i < 9) || i > 9)
            {
                if (self.player.playerState.slugcatCharacter == PlayerManager.GetCustomPlayer("Electric").SlugcatIndex)
                {
                    if (i == 12)
                    {
                        newContatiner.AddChild(sLeaser.sprites[i]);
                    }
                    else
                    {
                        if (i == 13)
                        {
                            newContatiner.AddChild(sLeaser.sprites[i]);
                        }
                        else
                        {
                            if (i == 14)
                            {
                                newContatiner.AddChild(sLeaser.sprites[i]);
                            }
                            else
                            {
                                rCam.ReturnFContainer("Foreground").AddChild(sLeaser.sprites[i]);
                            }
                        }
                    }
                }
                else
                {
                    if (i == 12)
                    {
                        rCam.ReturnFContainer("Bloom").AddChild(sLeaser.sprites[i]);
                    }
                    else
                    {
                        if (i == 13)
                        {
                            newContatiner.AddChild(sLeaser.sprites[i]);
                        }
                        else
                        {
                            if (i == 14)
                            {
                                newContatiner.AddChild(sLeaser.sprites[i]);
                            }
                            else
                            {
                                rCam.ReturnFContainer("Foreground").AddChild(sLeaser.sprites[i]);
                            }
                        }
                    }
                }
            }
            else
            {
                newContatiner.AddChild(sLeaser.sprites[i]);
            }
        }
    }

    private void ApplyPaletteHook(On.PlayerGraphics.orig_ApplyPalette orig, PlayerGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
    {
        if (self.player.playerState.slugcatCharacter == PlayerManager.GetCustomPlayer("Electric").SlugcatIndex)
        {
            for (int i = 0; i < EVars.Count; i++)
            {
                if (EVars[i].ply == self.player)
                {
                    Color color = PlayerGraphics.SlugcatColor(self.player.playerState.slugcatCharacter);
                    Color color2 = palette.blackColor;
                    if (self.malnourished > 0f)
                    {
                        float num = (!self.player.Malnourished) ? Mathf.Max(0f, self.malnourished - 0.005f) : self.malnourished;
                        color = Color.Lerp(color, Color.gray, 0.4f * num);
                        color2 = Color.Lerp(color2, Color.Lerp(Color.white, palette.fogColor, 0.5f), 0.2f * num * num);
                    }
                    color = EVars[i].ElectricBodyColor(EVars[i].chargedActive, EVars[i].chargedTimer, EVars[i].stunDelay);
                    color2 = palette.blackColor;
                    for (int s = 0; s < sLeaser.sprites.Length; s++)
                    {
                        sLeaser.sprites[s].color = color;
                    }
                    sLeaser.sprites[11].color = Color.Lerp(PlayerGraphics.SlugcatColor(self.player.playerState.slugcatCharacter), Color.white, 0.3f);
                    sLeaser.sprites[9].color = color2;
                    sLeaser.sprites[12].color = EVars[i].AntennaBaseColor(EVars[i].chargedActive);
                    sLeaser.sprites[13].color = EVars[i].AntennaTipColor(EVars[i].chargedActive, EVars[i].receivingMessage);
                    if (EVars[i].receivingMessage && EVars[i].chargedActive)
                    {
                        sLeaser.sprites[9].color = EVars[i].AntennaTipColor(true, true);
                    }
                }
            }
        }
        else
        {
            orig.Invoke(self, sLeaser, rCam, palette);
        }
    }

    private void DrawSpritesHook(On.PlayerGraphics.orig_DrawSprites orig, PlayerGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        self.ApplyPalette(sLeaser, rCam, rCam.currentPalette);
        orig.Invoke(self, sLeaser, rCam, timeStacker, camPos);
        if (self.player.playerState.slugcatCharacter == PlayerManager.GetCustomPlayer("Electric").SlugcatIndex)
        {
            for (int i = 0; i < EVars.Count; i++)
            {
                if (EVars[i].ply == self.player)
                {
                    if (!EVars[i].antennaActive)
                    {
                        sLeaser.sprites[13].scale = 0.01f;
                        sLeaser.sprites[12].scale = 0.01f;
                    }
                    else
                    {
                        sLeaser.sprites[13].scale = 2.3f;
                        sLeaser.sprites[12].scaleX = 0.95f;
                        sLeaser.sprites[12].scaleY = 4f;
                    }
                    sLeaser.sprites[12].x = sLeaser.sprites[3].x;
                    sLeaser.sprites[12].y = sLeaser.sprites[3].y + 2f;
                    sLeaser.sprites[12].rotation = sLeaser.sprites[3].rotation;
                    sLeaser.sprites[13].x = sLeaser.sprites[3].x;
                    sLeaser.sprites[13].y = sLeaser.sprites[3].y + 2.6f;
                    sLeaser.sprites[13].rotation = sLeaser.sprites[3].rotation;
                }
            }
        }
    }

    private void InitSpritesHook(On.PlayerGraphics.orig_InitiateSprites orig, PlayerGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
    {
        if (self.player.playerState.slugcatCharacter == PlayerManager.GetCustomPlayer("Electric").SlugcatIndex)
        {
            sLeaser.sprites = new FSprite[14];
        }
        else
        {
            sLeaser.sprites = new FSprite[12];
        }
        sLeaser.sprites[0] = new FSprite("BodyA", true);
        sLeaser.sprites[0].anchorY = 0.7894737f;
        sLeaser.sprites[1] = new FSprite("HipsA", true);
        TriangleMesh.Triangle[] tris = new TriangleMesh.Triangle[]
        {
            new TriangleMesh.Triangle(0, 1, 2),
            new TriangleMesh.Triangle(1, 2, 3),
            new TriangleMesh.Triangle(4, 5, 6),
            new TriangleMesh.Triangle(5, 6, 7),
            new TriangleMesh.Triangle(8, 9, 10),
            new TriangleMesh.Triangle(9, 10, 11),
            new TriangleMesh.Triangle(12, 13, 14),
            new TriangleMesh.Triangle(2, 3, 4),
            new TriangleMesh.Triangle(3, 4, 5),
            new TriangleMesh.Triangle(6, 7, 8),
            new TriangleMesh.Triangle(7, 8, 9),
            new TriangleMesh.Triangle(10, 11, 12),
            new TriangleMesh.Triangle(11, 12, 13)
        };
        TriangleMesh triangleMesh = new TriangleMesh("Futile_White", tris, false, false);
        sLeaser.sprites[2] = triangleMesh;
        sLeaser.sprites[3] = new FSprite("HeadA0", true);
        sLeaser.sprites[4] = new FSprite("LegsA0", true);
        sLeaser.sprites[4].anchorY = 0.25f;
        sLeaser.sprites[5] = new FSprite("PlayerArm0", true);
        sLeaser.sprites[5].anchorX = 0.9f;
        sLeaser.sprites[5].scaleY = -1f;
        sLeaser.sprites[6] = new FSprite("PlayerArm0", true);
        sLeaser.sprites[6].anchorX = 0.9f;
        sLeaser.sprites[7] = new FSprite("OnTopOfTerrainHand", true);
        sLeaser.sprites[8] = new FSprite("OnTopOfTerrainHand", true);
        sLeaser.sprites[8].scaleX = -1f;
        sLeaser.sprites[9] = new FSprite("FaceA0", true);
        sLeaser.sprites[11] = new FSprite("pixel", true);
        sLeaser.sprites[11].scale = 5f;
        sLeaser.sprites[10] = new FSprite("Futile_White", true);
        sLeaser.sprites[10].shader = rCam.game.rainWorld.Shaders["FlatLight"];
        if (self.player.playerState.slugcatCharacter == PlayerManager.GetCustomPlayer("Electric").SlugcatIndex)
        {
            sLeaser.sprites[12] = new FSprite("pixel", false);
            sLeaser.sprites[12].scaleY = 4f;
            sLeaser.sprites[12].scaleX = 1f;
            sLeaser.sprites[12].anchorY = -1.02f;
            sLeaser.sprites[13] = new FSprite("pixel", false);
            sLeaser.sprites[13].scale = 2.3f;
            sLeaser.sprites[13].anchorY = -3.1f;
        }
        self.AddToContainer(sLeaser, rCam, null);
    }

    public bool JollyPlayerCheck(int playerNumber)
    {
        switch (playerNumber)
        {
            case 0:
                return true;
            case 1:
                if (JollyCoop.JollyMod.config.player2Character == 0)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            case 2:
                if (JollyCoop.JollyMod.config.player3Character == 0)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            case 3:
                if (JollyCoop.JollyMod.config.player4Character == 0)
                {
                    return true;
                }
                else
                {
                    return false;
                }
        }
        return false;
    }

    private void PlayerCtorHook(On.Player.orig_ctor orig, Player self, AbstractCreature abstractCreature, World world)
    {
        orig.Invoke(self, abstractCreature, world);
        if (this.Enabled)
        {
            if (Type.GetType("JollyCoop.JollyMod, JollyCoop") != null)
            {
                if (JollyPlayerCheck(self.playerState.playerNumber) == true)
                {
                    if (self.playerState.slugcatCharacter != PlayerManager.GetCustomPlayer("Electric").SlugcatIndex)
                    {
                        self.playerState.slugcatCharacter = PlayerManager.GetCustomPlayer("Electric").SlugcatIndex;
                    }
                }
            }
            if (EVars == null)
            {
                EVars = new List<ElectricVars>();
                Debug.Log("--Initializing new Evars");
            }
            bool initTracker = true;
            for (int i = 0; i < EVars.Count; i++)
            {
                if (EVars[i].ply == self)
                {
                    initTracker = false;
                    Debug.Log("--Player already has Evars");
                    break;
                }
            }
            if (initTracker && self.playerState.slugcatCharacter == PlayerManager.GetCustomPlayer("Electric").SlugcatIndex)
            {
                EVars.Add(new ElectricVars(self));
                Debug.Log("--Evars added for new Player");
            }
        }
    }
    private void PlayerUpdateHook(On.Player.orig_Update orig, Player self, bool eu)
    {
        if (this.Enabled)
        {
            for (int i = 0; i < EVars.Count; i++)
            {
                if (EVars[i].ply == self)
                {
                    if (EVars[i].stunDelay > 0f)
                    {
                        if (EVars[i].stunDelay == 120f)
                        {
                            EVars[i].ply.room.PlaySound(SoundID.Centipede_Electric_Charge_LOOP, EVars[i].ply.firstChunk, false, 1f, 1f);
                        }
                        EVars[i].stunDelay -= 1f;
                    }
                    EVars[i].preCoord = EVars[i].ply.coord;
                    if (Input.GetKeyDown(KeyCode.L))
                    {
                        if (!EVars[i].receivingMessage)
                        {
                            EVars[i].receivingMessage = true;
                        }
                        else
                        {
                            EVars[i].receivingMessage = false;
                        }
                    }
                    if (EVars[i].ply.submerged || EVars[i].ply.bodyMode == Player.BodyModeIndex.Swimming)
                    {
                        if (EVars[i].chargedActive)
                        {
                            EVars[i].ply.room.AddObject(new UnderwaterShock(EVars[i].ply.room, EVars[i].ply, EVars[i].ply.firstChunk.pos, 14, Mathf.Lerp(200f, 1200f, 0.5f), 1.15f, EVars[i].ply, new Color(0.7f, 0.7f, 1f)));
                            EVars[i].ExitChargeMode();
                        }
                    }
                    if (EVars[i].ply.dead)
                    {
                        if (EVars[i].chargedActive)
                        {
                            EVars[i].ExitChargeMode();
                        }
                    }
                    EVars[i].CheckChargingUp();
                    if (EVars[i].chargedActive)
                    {
                        EVars[i].lightFlash = 4f;
                        EVars[i].ply.room.PlaySound(SoundID.Centipede_Shock, EVars[i].ply.firstChunk, false, 0.25f, 4f);
                        EVars[i].chargedTimer -= 1f;
                        if (EVars[i].chargedTimer == 0f)
                        {
                            EVars[i].chargedActive = false;
                            EVars[i].ExitChargeMode();
                        }
                    }
                }
            }
        }
        orig.Invoke(self, eu);
    }
    //ElectricVars object contains all the new variables unique to Electric Cat
    //A new ElectricVars is created for each player at the start of the cycle
    public class ElectricVars
    {
        public Player ply;
        public float lightFlash;
        public float buttonPressed;
        public float chargedTimer;
        public bool chargedActive;
        public bool antennaActive;
        public bool receivingMessage;
        public int animationCount;
        public int animationDelay;
        public float stunDelay;
        public int count;
        public WorldCoordinate preCoord;
        public Creature thrownBy;
        public bool jollyCheck;
        public Color playerColor = new Color(0.6509f, 0.3764f, 0f);
        public ElectricVars(Creature player)
        {
            this.ply = (player as Player);
            this.receivingMessage = false;
            this.antennaActive = true;
            this.stunDelay = 0f;
            this.animationCount = 0;
            this.animationDelay = 0;
            this.chargedActive = false;
            this.chargedTimer = 0f;
            this.buttonPressed = 0f;
            if (Type.GetType("JollyCoop.JollyMod, JollyCoop") != null)
            {
                JollyCheck();
            }
        }
        public void EnterChargedModeNoToll()
        {
            if (!this.chargedActive)
            {
                ply.room.AddObject(new Explosion.ExplosionLight(ply.firstChunk.pos, 100f, 0.85f, 6, new Color(0.7f, 0.7f, 1f)));
                this.buttonPressed = 0f;
                ply.room.PlaySound(SoundID.Bomb_Explode, ply.firstChunk, false, 0.7f, 1f);
                this.chargedActive = true;
                ply.gravity = 0.75f;
                ply.slugcatStats.runspeedFac = 1.5f;
                ply.slugcatStats.poleClimbSpeedFac = 2f;
                ply.slugcatStats.corridorClimbSpeedFac = 2f;
                ply.buoyancy = 0.95f;
                ply.slugcatStats.throwingSkill = 2;
                this.chargedTimer = 1200f;
            }
        }
        public void ExitChargeMode()
        {
            this.chargedActive = false;
            this.chargedTimer = 0f;
            ply.slugcatStats.runspeedFac = 0.9f;
            ply.slugcatStats.poleClimbSpeedFac = 1f;
            ply.slugcatStats.corridorClimbSpeedFac = 1f;
            ply.gravity = 0.9f;
            ply.buoyancy = 0.95f;
            ply.slugcatStats.throwingSkill = 1;
            ply.room.PlaySound(SoundID.Centipede_Shock, ply.firstChunk);
        }
        public void EnterChargedMode()
        {
            if (!this.chargedActive)
            {
                ply.room.AddObject(new Explosion.ExplosionLight(ply.firstChunk.pos, 150f, 0.85f, 6, new Color(0.7f, 0.7f, 1f)));
                this.buttonPressed = 0f;
                ply.room.PlaySound(SoundID.Bomb_Explode, ply.firstChunk, false, 0.7f, 1f);
                this.chargedActive = true;
                ply.gravity = 0.75f;
                ply.slugcatStats.runspeedFac = 1.5f;
                ply.slugcatStats.poleClimbSpeedFac = 2f;
                ply.slugcatStats.corridorClimbSpeedFac = 2f;
                for (int i = 0; i < ply.room.game.Players.Count; i++)
                {
                    (ply.room.game.Players[i].realizedCreature as Player).playerState.foodInStomach -= 4;
                }
                if (this.jollyCheck)
                {
                    JollyFoodAdjust();
                }
                ply.room.game.cameras[0].hud.foodMeter.NewShowCount(ply.playerState.foodInStomach);
                ply.buoyancy = 0.95f;
                ply.slugcatStats.throwingSkill = 2;
                if (ply.Karma > 5)
                {
                    this.chargedTimer = (float)(600 * ply.Karma);
                }
                else
                {
                    this.chargedTimer = 2400f;
                }
                if (ply.abstractCreature.world.game.IsStorySession)
                {
                    ply.abstractCreature.world.game.GetStorySession.saveState.totFood -= 4;
                }
            }
        }
        public void JollyFoodAdjust()
        {
            for (int i = 0; i < JollyCoop.PlayerHK.sharedFood.Length; i++)
            {
                JollyCoop.PlayerHK.sharedFood[i] = (byte)ply.playerState.foodInStomach;
            }
        }
        public void CheckChargingUp()
        {
            if (ply.playerState.foodInStomach >= 4)
            {
                if (ply.stun == 0 && !ply.dead && !ply.room.GetTile(ply.coord).verticalBeam && !this.chargedActive && ply.bodyMode == Player.BodyModeIndex.Stand)
                {
                    if (ply.input[0].y > 0)
                    {
                        this.buttonPressed += 1f;
                        ply.Blink(5);
                        if (this.buttonPressed >= 40f)
                        {
                            ply.room.PlaySound(SoundID.Centipede_Shock, ply.firstChunk, false, 0.6f, 0.8f);
                        }
                        if (this.buttonPressed >= 90f)
                        {
                            this.EnterChargedMode();
                            for (int i = 0; i < EVars.Count; i++)
                            {
                                if(EVars[i] != this)
                                {
                                    EVars[i].EnterChargedModeNoToll();
                                }
                            }
                        }
                    }
                }
                else
                {
                    this.buttonPressed = 0f;
                }
            }
        }
        public Color AntennaTipColor(bool chargedActive, bool receivingMessage)
        {
            Color color = Custom.HSL2RGB(0.8588f, 0.7333f, 0.4352f);
            Color color2 = Custom.HSL2RGB(receivingMessage ? ((float)this.count / 255f) : 0.8588f, 0.7294f, 0.74509f);
            if (this.count >= 250)
            {
                this.count = 0;
            }
            else
            {
                this.count += 10;
            }
            return chargedActive ? color2 : color;
        }
        public Color AntennaBaseColor(bool chargedActive)
        {
            Color color = Custom.HSL2RGB(0.8588f, 0.7333f, 0.5216f);
            Color color2 = Custom.HSL2RGB(0.8588f, 0.7294f, 0.6942f);
            return chargedActive ? color2 : color;
        }
        public Color ElectricBodyColor(bool chargedActive, float chargedTimer, float stunDelay)
        {
            Color color = playerColor;
            Color color2 = new Color(playerColor.r + 100f * 0.00349f, playerColor.g + 100f * 0.0025f, playerColor.b + 100f * 0.00125f);
            Color a = new Color(1f, 0.7176f, 0.3333f);
            Color result;
            if (chargedActive)
            {
                if (stunDelay > 0f)
                {
                    result = Color.Lerp(a, color, stunDelay / 300f);
                }
                else
                {
                    if (chargedTimer <= 100f)
                    {
                        result = new Color(playerColor.r + chargedTimer * 0.00349f, playerColor.g + chargedTimer * 0.0025f, playerColor.b + chargedTimer * 0.00125f);
                    }
                    else
                    {
                        result = color2;
                    }
                }
            }
            else
            {
                result = color;
            }
            return result;
        }
        public void JollyCheck()
        {
            if (Type.GetType("JollyCoop.JollyMod, JollyCoop") != null)
            {
                jollyCheck = true;
                playerColor = JollyCoop.JollyMod.config.playerBodyColors[ply.playerState.playerNumber];
            }
        }
    }
}

