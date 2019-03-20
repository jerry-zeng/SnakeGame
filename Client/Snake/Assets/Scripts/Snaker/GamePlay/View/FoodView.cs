using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GamePlay
{
    public class FoodView : BaseView 
    {
        protected Food _model;
        public Food Model
        {
            get{ return _model; }
        }

        public void Bind(Food model)
        {
            _model = model;

            Vector3 scale = new Vector3(1f, 1f, 1f);
            scale.x = _model.Radius*2f / _render.sprite.texture.width;
            scale.y = _model.Radius*2f / _render.sprite.texture.height;
            CachedTransform.localScale = scale;

            CachedTransform.localPosition = EntityPosToViewPos(_model.Position);
        }

        public override void Unbind()
        {
            base.Unbind();

            _model = null;
        }

        public override void DoUpdate(float dt)
        {
            base.DoUpdate(dt);

            if( _model == null )
                return;
        }
    }
}
