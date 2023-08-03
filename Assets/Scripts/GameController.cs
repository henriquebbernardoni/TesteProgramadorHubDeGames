using System.Collections;
using System.Collections.Generic;
using Unity.AI.Navigation;
using UnityEngine;

//Controlador geral do jogo e das fases,
//aqui est�o guardados diferentes dados da gameplay
public class GameController : MonoBehaviour
{
    private NavMeshSurface surface;

    private void Awake()
    {
        surface = GetComponent<NavMeshSurface>();
        BakeNavMesh();
    }

    //Dada a natureza flex�vel das fases, � necess�rio realizar
    //o Bake do NavMesh, sempre que uma fase se inicia.
    private void BakeNavMesh()
    {
        surface.BuildNavMesh();
    }
}