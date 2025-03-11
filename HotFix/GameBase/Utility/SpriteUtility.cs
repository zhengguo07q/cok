using TEngine;
using UnityEngine;
using UnityEngine.U2D;
using UnityEngine.UI;

namespace GameBase.Util
{
    public static class SpriteUtility
    {
        
        /// <summary>
        /// 从Texture创建Sprite
        /// </summary>
        /// <param name="texture"></param>
        /// <param name="pivot"></param>
        /// <param name="pixelsPerUnit"></param>
        /// <returns></returns>
        static public Sprite CreateSpriteFromTexture(Texture2D texture, Vector2 pivot = default, float pixelsPerUnit = 100f)
        {
            if (texture == null)
            {
                Debug.LogError("Texture is null.");
                return null;
            }

            Rect rect = new Rect(0, 0, texture.width, texture.height);
            Vector2 pivotPoint = pivot;

            return Sprite.Create(texture, rect, pivotPoint, pixelsPerUnit);
        }


        /// <summary>
        /// 从AssetBundle创建Sprite
        /// </summary>
        /// <param name="img"></param>
        /// <param name="location"></param>
        static async public void SetLoadSprite(Image img,string location)
        {
            var result = await GameModule.Resource.LoadAssetAsync<Texture>(location);
            img.sprite = SpriteUtility.CreateSpriteFromTexture((Texture2D)result);
        }

        ///<summary>
        ///从图集加载Sprite
        ///<summary>
        ///<param name="img">
        ///<param name="location"></param>
        /// <param name="spriteatlasLocation"></param>
        static async public void SetLoadSpriteByAtlas(Image img, string location, string spriteatlasLocation){
            var atlas = await GameModule.Resource.LoadAssetAsync<SpriteAtlas>(spriteatlasLocation);
            Sprite sprite = atlas.GetSprite(location);
            img.sprite = sprite;
        }
    }
}