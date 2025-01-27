using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

class Program
{
    static async Task Main(string[] args)
    {
        // Número de jugadores
        int playerCount = 5;
        var cancellationTokenSource = new CancellationTokenSource();

        // Crear y ejecutar tareas para cada jugador
        var playersTasks = new List<Task>();

        for (int i = 1; i <= playerCount; i++)
        {
            int playerId = i;
            var playerTask = Task.Run(() => PlayGame(playerId, cancellationTokenSource.Token));
            playersTasks.Add(playerTask);
        }

        // Esperar a que cualquier tarea termine
        var completedTask = await Task.WhenAny(playersTasks);
        Console.WriteLine($"La primera tarea completada fue: {completedTask.Id}");

        // Esperar a que todos los jugadores terminen
        await Task.WhenAll(playersTasks);
        Console.WriteLine("Todos los jugadores han terminado el juego.");
    }

    static async Task PlayGame(int playerId, CancellationToken token)
    {
        Console.WriteLine($"Jugador {playerId} empieza a jugar.");

        // Simulación de las tres misiones del jugador
        var missionTasks = new List<Task>();

        for (int i = 1; i <= 3; i++)
        {
            int missionId = i;
            var missionTask = Task.Run(() => CompleteMission(playerId, missionId, token), token);
            missionTasks.Add(missionTask);
        }

        // Esperar a que todas las misiones se completen o se cancelen
        try
        {
            await Task.WhenAll(missionTasks);
            Console.WriteLine($"Jugador {playerId} ha completado todas las misiones.");
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine($"Jugador {playerId} ha sido cancelado.");
        }
    }

    static async Task CompleteMission(int playerId, int missionId, CancellationToken token)
    {
        // Simulación de una misión con una probabilidad de fallo
        Random rand = new Random();
        int delay = rand.Next(1000, 3000); // Tiempo de espera aleatorio entre 1 y 3 segundos

        try
        {
            // Simulación de tarea asíncrona con retraso
            var missionTask = Task.Delay(delay, token);
            var completedTask = await Task.WhenAny(missionTask, Task.Delay(5000, token));

            if (completedTask == missionTask)
            {
                // Simulación de éxito o fracaso de la misión
                if (rand.NextDouble() > 0.5)
                {
                    Console.WriteLine($"Jugador {playerId} ha completado la misión {missionId} con éxito.");
                }
                else
                {
                    Console.WriteLine($"Jugador {playerId} falló la misión {missionId}.");
                    throw new Exception("Misión fallida");
                }
            }
            else
            {
                Console.WriteLine($"Jugador {playerId} ha tardado demasiado en completar la misión {missionId}.");
                throw new TimeoutException("Tiempo de espera agotado");
            }
        }
        catch (Exception ex)
        {
            // Continuación en caso de error (solo si la misión no fue completada)
            Task.Run(() => HandleMissionFailure(playerId, missionId, ex.Message));
        }
    }

    static void HandleMissionFailure(int playerId, int missionId, string errorMessage)
    {
        Console.WriteLine($"Jugador {playerId} ha fallado la misión {missionId}: {errorMessage}");
    }
}
