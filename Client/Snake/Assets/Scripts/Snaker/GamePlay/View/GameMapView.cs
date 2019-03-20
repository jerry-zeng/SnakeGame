using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Framework;

namespace GamePlay
{
    public class GameMapView : BaseView 
    {
        protected GameMap _model;
        public GameMap Model
        {
            get{ return _model; }
        }

        private Vector3 _modelCenter;
        public Vector3 ModelCenter
        {
            get{ return _modelCenter; }
        }

        private Rect _viewBound;
        public Rect MapViewBound
        {
            get{ return _viewBound; }
        }

        public void Bind(GameMap model)
        {
            _model = model;

            _modelCenter = _model.BoundRect.center.ToVector3();

            Vector3 scale = new Vector3(1f, 1f, 1f);
            scale.x = _model.Size.x / _render.sprite.texture.width + 0.02f;
            scale.y = _model.Size.y / _render.sprite.texture.height + 0.02f;
            CachedTransform.localScale = scale;

            CachedTransform.localPosition = Vector3.zero;

            _viewBound = new Rect(_model.OriginPosition-_model.BoundRect.center, _model.Size);
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
