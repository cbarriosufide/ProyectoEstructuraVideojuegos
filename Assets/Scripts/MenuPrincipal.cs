using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuPrincipal : MonoBehaviour

// Aquí vas a el nombre de la escena
{
	[SerializeField] private string nombreEscenaJuego;

	public void Jugar()
	{
		if (nombreEscenaJuego != "")
		{
			SceneManager.LoadScene(nombreEscenaJuego, LoadSceneMode.Single);
		}
		else
		{
			Debug.Log("No se asignó la escena del juego.");
		}
	}

	public void Salir()
	{
		Application.Quit();
		Debug.Log("Saliendo del juego...");
	}
}
