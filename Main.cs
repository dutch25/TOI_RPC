using MelonLoader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

[assembly: MelonInfo(typeof(TOI_RPC.ImmortalRPC), "Tale of Immortal Discord RPC", "1.0.0", "Dutch25")]
[assembly: MelonGame("Amazing Seasun Games", "Tale of Immortal")]

namespace TOI_RPC
{
    public class ImmortalRPC : MelonMod
    {
        public static UnitCtrlPlayer ActivePlayer = null;
        private static DiscordRpc.EventHandlers handlers;
        private static DateTime startTime;

        public override void OnApplicationStart()
        {
            MelonLogger.Msg("Initializing Tale of Immortal RPC (Native)...");

            handlers = new DiscordRpc.EventHandlers();
            handlers.readyCallback += ReadyCallback;
            handlers.disconnectedCallback += DisconnectedCallback;
            handlers.errorCallback += ErrorCallback;

            // Reverting to the AppID that was working before
            DiscordRpc.Initialize("1469590602105487390", ref handlers, true, null); 

            startTime = DateTime.UtcNow;

            MelonLogger.Msg("Discord RPC Initialized (Native)!");
        }

        private float lastUpdateTime = 0f;
        private const float UpdateInterval = 10f; // Update every 10 seconds

        public override void OnUpdate()
        {
            DiscordRpc.RunCallbacks();

            if (UnityEngine.Time.time - lastUpdateTime < UpdateInterval) return;
            lastUpdateTime = UnityEngine.Time.time;

            UpdateGamePresence();
        }

        private void UpdateGamePresence()
        {
            try
            {
                if (!ReflectAssembly("Assembly-CSharp", out var asm)) {
                    UpdateRPC("Loading...", "", "toilogo");
                    return;
                }

                var gType = asm.GetType("g");
                if (gType == null) {
                    UpdateRPC("In Menu", "", "toilogo");
                    return;
                }

                // Try to get world via property OR field
                object world = null;
                var worldField = gType.GetField("world", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
                if (worldField != null) world = worldField.GetValue(null);
                
                if (world == null) {
                    var worldProp = gType.GetProperty("world", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
                    if (worldProp != null) world = worldProp.GetValue(null);
                }

                if (world == null) {
                    UpdateRPC("In Menu", "", "toilogo");
                    return;
                }

                // world.run
                object run = null;
                var runField = world.GetType().GetField("run");
                if (runField != null) run = runField.GetValue(world);
                if (run == null) {
                    var runProp = world.GetType().GetProperty("run");
                    if (runProp != null) run = runProp.GetValue(world);
                }

                if (run == null) {
                    UpdateRPC("In Menu", "", "toilogo");
                    return;
                }

                string name = "Đang tải...";
                string state = "Đang tải...";

                // world.playerUnit
                object playerUnit = null;
                var pUnitProp = world.GetType().GetProperty("playerUnit");
                if (pUnitProp != null) playerUnit = pUnitProp.GetValue(world);

                if (playerUnit != null) {
                    // Try to go deeper into playerUnit.data
                    var pDataProp = playerUnit.GetType().GetProperty("data");
                    if (pDataProp == null) pDataProp = (System.Reflection.PropertyInfo)playerUnit.GetType().GetMember("data").FirstOrDefault(m => m is System.Reflection.PropertyInfo);
                    object pData = null;
                    if (pDataProp != null) pData = pDataProp.GetValue(playerUnit);
                    else {
                        var pDataField = playerUnit.GetType().GetField("data");
                        if (pDataField != null) pData = pDataField.GetValue(playerUnit);
                    }

                    if (pData != null) {
                        // Go into unitData
                        object uData = null;
                        var unitDataProp = pData.GetType().GetProperty("unitData");
                        if (unitDataProp != null) uData = unitDataProp.GetValue(pData);
                        if (uData == null) {
                            var unitDataField = pData.GetType().GetField("unitData");
                            if (unitDataField != null) uData = unitDataField.GetValue(pData);
                        }

                        if (uData != null) {
                            // Extract Name and Grade from propertyData
                            var propDataProp = uData.GetType().GetProperty("propertyData");
                            if (propDataProp != null) {
                                var propData = propDataProp.GetValue(uData);
                                if (propData != null) {
                                    // Name
                                    var nameProp = propData.GetType().GetProperty("name");
                                    if (nameProp != null) {
                                        var val = nameProp.GetValue(propData);
                                        if (val != null) {
                                            if (val is UnhollowerBaseLib.Il2CppStringArray strArray) {
                                                name = string.Join("", strArray);
                                            } else {
                                                name = val.ToString();
                                            }
                                        }
                                    }

                                    // Grade (Realm)
                                    var gradeProp = propData.GetType().GetProperty("gradeID");
                                    if (gradeProp != null) {
                                        var val = gradeProp.GetValue(propData);
                                        if (val != null) {
                                            state = GetRealmName(Convert.ToInt32(val));
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                // Years and Months from run.roundMonth
                int totalMonths = 0;
                var monthProp = run.GetType().GetProperty("roundMonth");
                if (monthProp != null) totalMonths = (int)monthProp.GetValue(run);
                int years = (totalMonths / 12) + 1;
                int months = (totalMonths % 12) + 1;

                // Final update with Full Info
                UpdateRPC($"{name} (Năm {years}, Tháng {months})", state, "toilogo");
            }
            catch (Exception)
            {
                UpdateRPC("Trong Menu", "", "toilogo");
            }
        }

        private string GetRealmName(int gradeId)
        {
            string realmName;
            string phaseName;

            if (gradeId <= 3)
            {
                realmName = "Luyện Khí Cảnh";
                phaseName = gradeId == 1 ? "Sơ Kỳ" : (gradeId == 2 ? "Trung Kỳ" : "Hậu Kỳ");
            }
            else if (gradeId <= 8)
            {
                realmName = "Trúc Cơ Cảnh";
                if (gradeId <= 6) phaseName = "Sơ Kỳ";
                else if (gradeId == 7) phaseName = "Trung Kỳ";
                else phaseName = "Hậu Kỳ";
            }
            else if (gradeId <= 11)
            {
                realmName = "Kết Tinh Cảnh";
                phaseName = gradeId == 9 ? "Sơ Kỳ" : (gradeId == 10 ? "Trung Kỳ" : "Hậu Kỳ");
            }
            else if (gradeId <= 18)
            {
                realmName = "Kim Đan Cảnh";
                if (gradeId <= 16) phaseName = "Sơ Kỳ";
                else if (gradeId == 17) phaseName = "Trung Kỳ";
                else phaseName = "Hậu Kỳ";
            }
            else if (gradeId <= 21)
            {
                realmName = "Cụ Linh Cảnh";
                phaseName = gradeId == 19 ? "Sơ Kỳ" : (gradeId == 20 ? "Trung Kỳ" : "Hậu Kỳ");
            }
            else if (gradeId <= 26)
            {
                realmName = "Nguyên Anh Cảnh";
                if (gradeId <= 24) phaseName = "Sơ Kỳ";
                else if (gradeId == 25) phaseName = "Trung Kỳ";
                else phaseName = "Hậu Kỳ";
            }
            else if (gradeId <= 29)
            {
                realmName = "Hoá Thần Cảnh";
                phaseName = gradeId == 27 ? "Sơ Kỳ" : (gradeId == 28 ? "Trung Kỳ" : "Hậu Kỳ");
            }
            else if (gradeId <= 35)
            {
                realmName = "Ngộ Đạo Cảnh";
                if (gradeId <= 32) phaseName = "Sơ Kỳ";
                else if (gradeId == 33) phaseName = "Trung Kỳ";
                else phaseName = "Hậu Kỳ";
            }
            else if (gradeId <= 38)
            {
                realmName = "Vũ Hóa Cảnh";
                phaseName = gradeId == 36 ? "Sơ Kỳ" : (gradeId == 37 ? "Trung Kỳ" : "Hậu Kỳ");
            }
            else
            {
                realmName = "Đăng Tiên Cảnh";
                if (gradeId <= 42) phaseName = "Sơ Kỳ";
                else if (gradeId == 43) phaseName = "Trung Kỳ";
                else phaseName = "Hậu Kỳ";
            }

            return realmName + " (" + phaseName + ")";
        }

        [HarmonyLib.HarmonyPatch(typeof(WorldMgr), "IntoWorld")]
        public static class IntoWorldPatch
        {
            public static void Postfix(WorldMgr __instance)
            {
                MelonLogger.Msg("[PATCH] Entered World. Presence should update soon.");
            }
        }

        private bool ReflectAssembly(string name, out System.Reflection.Assembly asm)
        {
            asm = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(a => a.GetName().Name == name);
            return asm != null;
        }

        public override void OnApplicationQuit()
        {
            DiscordRpc.Shutdown();
        }

        private void UpdateRPC(string details, string state, string largeImg)
        {
            var presence = new DiscordRpc.RichPresence()
            {
                details = details,
                state = state,
                startTimestamp = (long)(startTime - new DateTime(1970, 1, 1)).TotalSeconds,
                largeImageKey = largeImg,
                largeImageText = "Tale of Immortal"
            };

            DiscordRpc.UpdatePresence(ref presence);
            MelonLogger.Msg($"RPC Pending Update: {details} - {state}");
        }

        private void ReadyCallback(ref DiscordRpc.DiscordUser user)
        {
            MelonLogger.Msg($"Discord RPC Ready! User: {user.username}");
        }

        private void DisconnectedCallback(int errorCode, string message)
        {
            MelonLogger.Error($"Discord RPC Disconnected: {errorCode} - {message}");
        }

        private void ErrorCallback(int errorCode, string message)
        {
            MelonLogger.Error($"Discord RPC Error: {errorCode} - {message}");
        }
    }
}
