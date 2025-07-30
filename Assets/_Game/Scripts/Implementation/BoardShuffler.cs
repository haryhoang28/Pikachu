using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class BoardShuffler : IBoardShuffler
{
    private readonly IGridManager _gridManager;
    private readonly IPokemonSpawner _pokemonSpawner;
    private readonly IBoardAnalyzer _boardAnalyzer; // Để kiểm tra sau khi shuffle

    public BoardShuffler(IGridManager gridManager, IPokemonSpawner pokemonSpawner, IBoardAnalyzer boardAnalyzer)
    {
        _gridManager = gridManager;
        _pokemonSpawner = pokemonSpawner;
        _boardAnalyzer = boardAnalyzer;
    }

    public IEnumerator ShuffleBoardRoutine(Transform parentTransform)
    {
        int maxAttempts = 100;
        float delayBetweenAttempts = 0.2f;

        for (int attempts = 0; attempts < maxAttempts; attempts++)
        {
            List<Pokemon> activePokemons = _gridManager.GetAllActivePokemons();
            // Shuffle the types of activePokemons
            List<PokemonType> types = activePokemons.Select(p => p.Type).ToList();
            ShuffleList(types);

            // Reassign shuffled types and update sprite
            for (int i = 0; i < activePokemons.Count; i++)
            {
                activePokemons[i].Type = types[i];
                _pokemonSpawner.SetPokemonSprite(activePokemons[i], types[i]);
            }
            if (_boardAnalyzer.HasPossibleMatches())
            {
                yield break;
            }
            yield return new WaitForSeconds(delayBetweenAttempts);
        }
    }
    public void ShuffleList<T>(List<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int randomIndex = Random.Range(0, i + 1);
            (list[randomIndex], list[i]) = (list[i], list[randomIndex]);
        }
    }
}