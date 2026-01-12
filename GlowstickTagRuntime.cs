using Enemies;
using System;
using UnityEngine;

namespace BioTrackerBeacon
{
    internal sealed class GlowstickTagRuntime : MonoBehaviour
    {
        private const int HibernateStateValue = 14;

        private int _goId;
        private bool _started;
        private float _nextScanTime;

        public void Init(int goId)
        {
            _goId = goId;
        }

        public void Begin()
        {
            if (_started) return;
            _started = true;

            // Small random offset so multiple glowsticks do not always scan on the exact same frame.
            _nextScanTime = Time.realtimeSinceStartup + UnityEngine.Random.Range(0f, 0.05f);
        }

        private void Update()
        {
            if (!_started) return;

            float now = Time.realtimeSinceStartup;
            float interval = Mathf.Max(0.05f, BioStickTracker.CfgTickInterval.Value);
            if (now < _nextScanTime)
                return;

            _nextScanTime = now + interval;

            Scan();
        }

        private void Scan()
        {
            float radius = Mathf.Max(0f, BioStickTracker.CfgTagRadius.Value);
            if (radius <= 0f)
                return;

            Vector3 pos = transform.position;

            if (BioStickTracker.CfgDebugLog.Value)
                BioStickTracker.Logger?.LogInfo($"[Glowstick] Scan start goId={_goId} pos={pos} radius={radius:F2} tagSleepers={BioStickTracker.CfgTagSleepers.Value} tagShadows={BioStickTracker.CfgTagShadows.Value}");

            Collider[] cols;
            try
            {
                cols = Physics.OverlapSphere(pos, radius, ~0, QueryTriggerInteraction.Collide);
            }
            catch (Exception e)
            {
                if (BioStickTracker.CfgDebugLog.Value)
                    BioStickTracker.Logger?.LogWarning($"[Glowstick] OverlapSphere failed goId={_goId}: {e.GetType().Name}");
                return;
            }

            int colliderCount = cols?.Length ?? 0;

            int checkedEnemies = 0;
            int skippedInactive = 0;
            int skippedTagged = 0;
            int skippedShadow = 0;
            int skippedSleeper = 0;
            int tagRequests = 0;

            for (int i = 0; i < colliderCount; i++)
            {
                var col = cols[i];
                if (col == null) continue;

                EnemyAgent? enemy;
                try { enemy = col.GetComponentInParent<EnemyAgent>(); }
                catch { enemy = null; }

                if (enemy == null) continue;

                checkedEnemies++;

                try
                {
                    if (!enemy.gameObject.activeInHierarchy)
                    {
                        skippedInactive++;
                        continue;
                    }
                }
                catch { }

                bool isTagged;
                try { isTagged = enemy.IsTagged; }
                catch { isTagged = false; }

                if (isTagged)
                {
                    skippedTagged++;
                    continue;
                }

                if (!BioStickTracker.CfgTagShadows.Value)
                {
                    bool isShadow;
                    try { isShadow = enemy.IsInvisible(); }
                    catch { isShadow = false; }

                    if (isShadow)
                    {
                        skippedShadow++;
                        continue;
                    }
                }

                if (!BioStickTracker.CfgTagSleepers.Value && IsHibernating(enemy))
                {
                    skippedSleeper++;
                    continue;
                }

                ToolSyncManager.WantToTagEnemy(enemy);
                tagRequests++;

                if (BioStickTracker.CfgDebugLog.Value)
                {
                    int enemyId = 0;
                    try { enemyId = enemy.GetInstanceID(); } catch { }
                    BioStickTracker.Logger?.LogInfo($"[Glowstick] Tag request goId={_goId} enemyId={enemyId}");
                }
            }

            if (BioStickTracker.CfgDebugLog.Value)
            {
                BioStickTracker.Logger?.LogInfo(
                    $"[Glowstick] Scan end goId={_goId} cols={colliderCount} enemies={checkedEnemies} " +
                    $"tagReq={tagRequests} skipInactive={skippedInactive} skipTagged={skippedTagged} skipShadow={skippedShadow} skipSleeper={skippedSleeper}"
                );
            }
        }

        private static bool IsHibernating(EnemyAgent enemy)
        {
            if (enemy == null) return false;

            EnemyLocomotion? locomotion = null;

            try { locomotion = enemy.GetComponent<EnemyLocomotion>(); } catch { locomotion = null; }
            if (locomotion == null) return false;

            try { return (int)locomotion.CurrentStateEnum == HibernateStateValue; }
            catch { return false; }
        }
    }
}
