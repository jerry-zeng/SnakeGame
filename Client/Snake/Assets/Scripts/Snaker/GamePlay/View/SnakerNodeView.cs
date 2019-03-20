using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GamePlay
{
    public class SnakerNodeView : BaseView 
    {
        protected SnakerNode _model;
        Vector3 angles = new Vector3();

        public void Bind(SnakerNode model)
        {
            _model = model;

            Vector3 scale = new Vector3(1f, 1f, 1f);
            scale.x = _model.Radius*2f / _render.sprite.texture.width;
            scale.y = _model.Radius*2f / _render.sprite.texture.height;
            CachedTransform.localScale = scale;

            DoUpdate(0f);
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

            // position
            CachedTransform.localPosition = EntityPosToViewPos(_model.Position);

            // rotation
            Vector3 dir = _model.Position - _model.PrePosition;
            angles.z = (float)(Mathf.Atan2(dir.y, dir.x) * 180f / Mathf.PI) - 90f;
            CachedTransform.localEulerAngles = angles;

            // scale

        }
    }
}
