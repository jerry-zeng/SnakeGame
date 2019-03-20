using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Framework.UI
{
    public class UIAssetManager
    {

        public delegate void OnTextureReady(string textureName, Texture texture);
        public delegate void OnPopupReady(string popupName, GameObject uiPopup);


        /// <summary>
        /// The dict that contains persist textures placed on folder /Resources and they won't be updated by patchs.
        /// </summary>
        private static Dictionary<string, Texture> persistTextureDict = new Dictionary<string, Texture>();

        /// <summary>
        /// The dict that contains non-persist textures placed on AssetBundle and they will be updated by patchs.
        /// </summary>
        private static Dictionary<string, Texture> nonPersistTextureDict = new Dictionary<string, Texture>();


        #region UI Textures
        public static string GetDefaultTexturePath(string textureName)
        {
            return "UI/Textures/" + textureName;
        }

        public static void RequestUITexture(string textureName, OnTextureReady onTextureReady, bool persist = false)
        {
            if(string.IsNullOrEmpty(textureName))
                return;
            if(onTextureReady == null)
                return;

            Texture texture = null;

            if(persistTextureDict.TryGetValue(textureName, out texture))
            {
                if(texture == null)
                    persistTextureDict.Remove(textureName);
            }
            else if(nonPersistTextureDict.TryGetValue(textureName, out texture)) 
            {
                if(texture == null)
                    nonPersistTextureDict.Remove(textureName);
            }

            if(texture == null) {
                // TODO: manage the Load method later.
                texture = Resources.Load<Texture>(GetDefaultTexturePath(textureName));

                if(persist)
                    persistTextureDict.Add(textureName, texture);
                else
                    nonPersistTextureDict.Add(textureName, texture);
            }

            onTextureReady(textureName, texture);
        }
        #endregion

        #region UI Popups
        public static string GetDefaultPopupPath(string popupName)
        {
            return "UI/Popups/" + popupName;
        }


        public static void RequestUIPopup(string popupName, OnPopupReady onPopupReady)
        {
            if(string.IsNullOrEmpty(popupName))
                return;
            if(onPopupReady == null)
                return;

            // TODO: manage the Load method later.
            GameObject prefab = Resources.Load<GameObject>( GetDefaultPopupPath(popupName) );

            GameObject go = UI_Instantiate_GameObject(prefab, popupName);

            onPopupReady(popupName, go);
        }


        public static GameObject UI_Instantiate_GameObject(Object prefab, string newName = "")
        {
            if(prefab == null)
                return null;

            GameObject go = Object.Instantiate(prefab) as GameObject;
            if(go != null)
                go.name = string.IsNullOrEmpty(newName)? prefab.name : newName;

            return go;
        }

        public static void UI_Destroy_GameObject(GameObject go)
        {
            if(go == null)
                return;

            go.transform.SetParent(null);
            GameObject.Destroy(go);
        }
        #endregion


        #region Unload Textures
        public static void UnloadNonPersistTextures()
        {
            var iter = nonPersistTextureDict.GetEnumerator();
            while(iter.MoveNext()) 
            {
                Texture texture = iter.Current.Value;
                if(texture != null)
                    Resources.UnloadAsset(texture);
            }
            nonPersistTextureDict.Clear();
        }
        public static void UnloadPersistTextures()
        {
            var iter = persistTextureDict.GetEnumerator();
            while(iter.MoveNext()) 
            {
                Texture texture = iter.Current.Value;
                if(texture != null)
                    Resources.UnloadAsset(texture);
            }
            persistTextureDict.Clear();
        }
        public static void UnloadAllTextures()
        {
            UnloadNonPersistTextures();
            UnloadPersistTextures();
        }


        public static void UnloadDownloadedTexture(Texture texture)
        {
            // explicitly unload texture whether from persistent or non-persist
            if(texture != null) 
            {
                string textureName = texture.name;
                if(persistTextureDict.ContainsKey(textureName))
                {
                    Resources.UnloadAsset(texture);
                    persistTextureDict.Remove(textureName);
                }
                else if(nonPersistTextureDict.ContainsKey(textureName))
                {
                    Resources.UnloadAsset(texture);
                    nonPersistTextureDict.Remove(textureName);
                }
            }
        }

        public static void UnloadTexturesOnGameObject(GameObject go)
        {
            if(go != null)
            {
                RawImage[] backgrounds = go.GetComponentsInChildren<RawImage>(true);
                for(int i = 0; i < backgrounds.Length; i++)
                {
                    RawImage bg = backgrounds[i];
                    if( bg.texture != null )
                    {
                        if( !persistTextureDict.ContainsKey(bg.texture.name) ) // exclude persist list
                        {
                            Resources.UnloadAsset(bg.texture);
                        }
                        bg.texture = null;
                    }
                }
                backgrounds = null;
            }
        }
        #endregion

    }
}
