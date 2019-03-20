using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Framework.UI.Components
{
    [CustomEditor(typeof(UIListView))]
    public class UIListViewInspector : Editor 
    {
        public override void OnInspectorGUI()
        {
            //base.OnInspectorGUI();

            serializedObject.Update();

            UIListView listView = target as UIListView;

            listView.direction = (UIListView.Direction)EditorGUILayout.EnumPopup("Direction", listView.direction);

            SerializedProperty sp = serializedObject.FindProperty("ItemPrefab");
            if( sp != null ){
                EditorGUILayout.PropertyField( sp, new GUIContent("Item Template"));
            }

            if( listView.direction == UIListView.Direction.Vertical ){
                listView.colOrRowCount = EditorGUILayout.IntField("Column Count", listView.colOrRowCount);
            }
            else{
                listView.colOrRowCount = EditorGUILayout.IntField("Row Count", listView.colOrRowCount);
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}
