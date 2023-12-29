using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Unity.Netcode;
using UnityEngine;
using Object = UnityEngine.Object;

namespace CoDOnCollect.Patches
{
    [HarmonyPatch(typeof(HUDManager))]
    internal class HUDManagerPatch
    {


        [HarmonyPatch(nameof(HUDManager.DisplayNewScrapFound))]
        [HarmonyPrefix]
        static void CauseOfDeathOnCollectPrefixPatch(   ref bool __runOriginal,
                                                        ref HUDManager __instance)
        {
            if (__runOriginal &&
                __instance.itemsToBeDisplayed.Count > 0 &&
                __instance.itemsToBeDisplayed[0] != null &&
                __instance.itemsToBeDisplayed[0].itemProperties.spawnPrefab != null &&
                __instance.itemsToBeDisplayed[0] is RagdollGrabbableObject)
            {
                AccessTools.Method(typeof(IEnumerator), "displayScrapTimer");

                // If the item is a corpse, replace the original function
                __runOriginal = false;

                // Copy of original code
                __instance.UIAudio.PlayOneShot(__instance.displayCollectedScrapSFXSmall);
                GameObject gameObject = Object.Instantiate(__instance.itemsToBeDisplayed[0].itemProperties.spawnPrefab, __instance.ScrapItemBoxes[__instance.nextBoxIndex].itemObjectContainer);
                Object.Destroy(gameObject.GetComponent<NetworkObject>());
                Object.Destroy(gameObject.GetComponent<GrabbableObject>());
                Object.Destroy(gameObject.GetComponent<Collider>());
                gameObject.transform.localPosition = Vector3.zero;
                gameObject.transform.localScale = gameObject.transform.localScale * 4f;
                gameObject.transform.rotation = Quaternion.Euler(__instance.itemsToBeDisplayed[0].itemProperties.restingRotation);
                Renderer[] componentsInChildren = gameObject.GetComponentsInChildren<Renderer>();
                for (int i = 0; i < componentsInChildren.Length; i++)
                {
                    if (componentsInChildren[i].gameObject.layer != 22)
                    {
                        Material[] sharedMaterials = componentsInChildren[i].sharedMaterials;
                        componentsInChildren[i].rendererPriority = 70;
                        for (int j = 0; j < sharedMaterials.Length; j++)
                        {
                            sharedMaterials[j] = __instance.hologramMaterial;
                        }
                        componentsInChildren[i].sharedMaterials = sharedMaterials;
                        componentsInChildren[i].gameObject.layer = 5;
                    }
                }
                __instance.ScrapItemBoxes[__instance.nextBoxIndex].itemDisplayAnimator.SetTrigger("collect");

                // Edited code to add cause of death to collection message
                RagdollGrabbableObject ragdollGrabbableObject = __instance.itemsToBeDisplayed[0] as RagdollGrabbableObject;
                if (ragdollGrabbableObject != null && ragdollGrabbableObject.ragdoll != null)
                {
                    //___ScrapItemBoxes[___nextBoxIndex].headerText.text = ragdollGrabbableObject.ragdoll.playerScript.playerUsername + " collected!";
                    string causeOfDeathString = "";
                    switch (ragdollGrabbableObject.ragdoll.causeOfDeath)
                    {
                        case CauseOfDeath.Abandoned:
                            causeOfDeathString = "(Left Behind)";
                            break;

                        case CauseOfDeath.Blast:
                            causeOfDeathString = "(They Blew Up)";
                            break;

                        case CauseOfDeath.Bludgeoning:
                            causeOfDeathString = "(Bonked)";
                            break;

                        case CauseOfDeath.Crushing:
                            causeOfDeathString = "(lmao get laddered)";
                            break;

                        case CauseOfDeath.Drowning:
                            causeOfDeathString = "(Couldn't Swim)";
                            break;

                        case CauseOfDeath.Electrocution:
                            causeOfDeathString = "(Shocking)";
                            break;

                        case CauseOfDeath.Gravity:
                            causeOfDeathString = "(Hit the ground too hard)";
                            break;

                        case CauseOfDeath.Gunshots:
                            causeOfDeathString = "(Full of Bullets)";
                            break;

                        case CauseOfDeath.Kicking:
                            causeOfDeathString = "(Cracked Nuts)";
                            break;

                        case CauseOfDeath.Mauling:
                            causeOfDeathString = "(Took a beating)";
                            break;

                        case CauseOfDeath.Strangulation:
                            causeOfDeathString = "(Hugged a Bracken)";
                            break;

                        case CauseOfDeath.Suffocation:
                            causeOfDeathString = "(Got snare'd when they should have flea'd)";
                            break;

                        default:
                            causeOfDeathString = "(Unknown Cause of Death)";
                            break;
                    }
                    __instance.ScrapItemBoxes[__instance.nextBoxIndex].headerText.text = ragdollGrabbableObject.ragdoll.playerScript.playerUsername + " collected! " + causeOfDeathString;
                }
                else
                {
                    //___ScrapItemBoxes[___nextBoxIndex].headerText.text = "Body collected!";
                    __instance.ScrapItemBoxes[__instance.nextBoxIndex].headerText.text = "Body collected! (Unknown Cause of Death)";
                }

                // Back to original code
                if (__instance.boxesDisplaying > 0)
                {
                    __instance.ScrapItemBoxes[__instance.nextBoxIndex].UIContainer.anchoredPosition = new Vector2(__instance.ScrapItemBoxes[__instance.nextBoxIndex].UIContainer.anchoredPosition.x, __instance.ScrapItemBoxes[__instance.bottomBoxIndex].UIContainer.anchoredPosition.y - 124f);
                }
                else
                {
                    __instance.ScrapItemBoxes[__instance.nextBoxIndex].UIContainer.anchoredPosition = new Vector2(__instance.ScrapItemBoxes[__instance.nextBoxIndex].UIContainer.anchoredPosition.x, __instance.bottomBoxYPosition);
                }
                __instance.bottomBoxIndex = __instance.nextBoxIndex;
                __instance.StartCoroutine(__instance.displayScrapTimer(gameObject));
                __instance.playScrapDisplaySFX();
                __instance.boxesDisplaying++;
                __instance.nextBoxIndex = (__instance.nextBoxIndex + 1) % 3;
                __instance.itemsToBeDisplayed.RemoveAt(0);
            }
        }

        /*
        public void DisplayNewScrapFound()
        {
            if (itemsToBeDisplayed.Count <= 0)
            {
                return;
            }
            if (itemsToBeDisplayed[0] == null || itemsToBeDisplayed[0].itemProperties.spawnPrefab == null)
            {
                itemsToBeDisplayed.Clear();
                return;
            }
            if (itemsToBeDisplayed[0].scrapValue < 80)
            {
                UIAudio.PlayOneShot(displayCollectedScrapSFXSmall);
            }
            else
            {
                UIAudio.PlayOneShot(displayCollectedScrapSFX);
            }
            GameObject gameObject = Object.Instantiate(itemsToBeDisplayed[0].itemProperties.spawnPrefab, ScrapItemBoxes[nextBoxIndex].itemObjectContainer);
            Object.Destroy(gameObject.GetComponent<NetworkObject>());
            Object.Destroy(gameObject.GetComponent<GrabbableObject>());
            Object.Destroy(gameObject.GetComponent<Collider>());
            gameObject.transform.localPosition = Vector3.zero;
            gameObject.transform.localScale = gameObject.transform.localScale * 4f;
            gameObject.transform.rotation = Quaternion.Euler(itemsToBeDisplayed[0].itemProperties.restingRotation);
            Renderer[] componentsInChildren = gameObject.GetComponentsInChildren<Renderer>();
            for (int i = 0; i < componentsInChildren.Length; i++)
            {
                if (componentsInChildren[i].gameObject.layer != 22)
                {
                    Material[] sharedMaterials = componentsInChildren[i].sharedMaterials;
                    componentsInChildren[i].rendererPriority = 70;
                    for (int j = 0; j < sharedMaterials.Length; j++)
                    {
                        sharedMaterials[j] = hologramMaterial;
                    }
                    componentsInChildren[i].sharedMaterials = sharedMaterials;
                    componentsInChildren[i].gameObject.layer = 5;
                }
            }
            ScrapItemBoxes[nextBoxIndex].itemDisplayAnimator.SetTrigger("collect");
            if (itemsToBeDisplayed[0] is RagdollGrabbableObject)
            {
                RagdollGrabbableObject ragdollGrabbableObject = itemsToBeDisplayed[0] as RagdollGrabbableObject;
                if (ragdollGrabbableObject != null && ragdollGrabbableObject.ragdoll != null)
                {
                    ScrapItemBoxes[nextBoxIndex].headerText.text = ragdollGrabbableObject.ragdoll.playerScript.playerUsername + " collected!";
                }
                else
                {
                    ScrapItemBoxes[nextBoxIndex].headerText.text = "Body collected!";
                }
            }
            else
            {
                ScrapItemBoxes[nextBoxIndex].headerText.text = itemsToBeDisplayed[0].itemProperties.itemName + " collected!";
            }
            ScrapItemBoxes[nextBoxIndex].valueText.text = $"Value: ${itemsToBeDisplayed[0].scrapValue}";
            if (boxesDisplaying > 0)
            {
                ScrapItemBoxes[nextBoxIndex].UIContainer.anchoredPosition = new Vector2(ScrapItemBoxes[nextBoxIndex].UIContainer.anchoredPosition.x, ScrapItemBoxes[bottomBoxIndex].UIContainer.anchoredPosition.y - 124f);
            }
            else
            {
                ScrapItemBoxes[nextBoxIndex].UIContainer.anchoredPosition = new Vector2(ScrapItemBoxes[nextBoxIndex].UIContainer.anchoredPosition.x, bottomBoxYPosition);
            }
            bottomBoxIndex = nextBoxIndex;
            StartCoroutine(displayScrapTimer(gameObject));
            playScrapDisplaySFX();
            boxesDisplaying++;
            nextBoxIndex = (nextBoxIndex + 1) % 3;
            itemsToBeDisplayed.RemoveAt(0);
        }
         */

    }
}
