using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GamePlay
{
    public class Entity 
    {
        protected int _id;

        /// <summary>
        /// Gets the entity ID.
        /// </summary>
        /// <value>The I.</value>
        public int ID
        {
            get{ return _id; }
        }

        protected int _radius = 8;
        /// <summary>
        /// Gets the radius, all entities are circle.
        /// </summary>
        public int Radius
        {
            get{ return _radius; }
        }

        protected Vector3 _position;
        /// <summary>
        /// Gets the position in current frame.
        /// </summary>
        public Vector3 Position 
        { 
            get{ return _position; } 
        }

        protected Vector3 _prePosition;
        /// <summary>
        /// Gets the position in previous frame.
        /// </summary>
        public Vector3 PrePosition 
        { 
            get{ return _prePosition; } 
        }

    }
}
