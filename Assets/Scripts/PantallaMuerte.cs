using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PantallaMuerte : MonoBehaviour
{
	// Escena del menú principal
	private string nombreMenuPrincipal = "Menu Principal";

	// !!!!!ESCENA PARA REINICIAR JUEGO
	[SerializeField] private string nombreEscenaJuego = "";

	// BOTÓN: Volver al menú principal
	public void IrAlMenu()
	{
		if (nombreMenuPrincipal != "")
		{
			SceneManager.LoadScene(nombreMenuPrincipal);
			Debug.Log("Cargando menu principal");
		}
		else
		{
			Debug.Log("No se ha asignado la escena del menú principal.");
		}
	}

	// BOTÓN: Jugar de nuevo / Reintentar
	public void JugarDeNuevo()
	{
		if (nombreEscenaJuego != "")
		{
			SceneManager.LoadScene(nombreEscenaJuego);
			Debug.Log("cargando escena de juego");
		}
		else
		{
			Debug.Log("No se ha asignado la escena inicial del juego.");
		}
	}
}
