# Dokumentacja Wtyczki CS2: FragHub Quests

## 1. Wstęp
Wtyczka **FragHub Quests** dla serwerów Counter-Strike 2 umożliwia graczom wykonywanie zadań (questów) przypisanych do ich kont na platformie FragHub. Wtyczka komunikuje się z zewnętrznym backendem poprzez API w celu pobierania zadań, aktualizowania postępów oraz łączenia kont graczy.

## 2. Funkcjonalności
1. **Pobieranie zadań**: Serwer cyklicznie i na żądanie pobiera listę aktywnych questów dla graczy z backendu (API FragHub).
2. **Lista zadań pod komendą**: Gracze mogą sprawdzić swoje aktualne zadania oraz nagrody (np. EXP) za pomocą dedykowanej komendy w grze.
3. **Śledzenie postępów w czasie rzeczywistym**: Wtyczka nasłuchuje zdarzeń w grze (np. zabójstwa, headshoty, asysty, zadane obrażenia, rozbrojenie bomby) i na bieżąco aktualizuje postępy w zadaniach.
4. **Informacje o ukończeniu**: Po ukończeniu zadania gracz otrzymuje powiadomienie (np. na czacie, ekranie), a wtyczka wysyła informację o sukcesie do backendu.
5. **Integracja kont**: Mechanizm łączenia konta Steam gracza z kontem na FragHub bezpośrednio na serwerze (np. poprzez wygenerowanie kodu lub komendę uwierzytelniającą).

## 3. Komendy dla Graczy
- `!quests` / `/quests` - Otwiera listę aktualnych zadań gracza wraz z postępem i nagrodą (EXP).
- `!link` / `/link <token>` - Łączy konto Steam na serwerze z kontem FragHub za pomocą tokenu pobranego z huba.

## 4. Komunikacja z API (Proponowany schemat)
Wtyczka wymaga komunikacji HTTP/HTTPS z REST API backendu FragHub.

### Endpoints (FragHub Action Routing)
* `GET /fg_addons/api/api.php?action=server_quests_get&steamId={steamId}`
  * Zwraca listę zadań przypisanych do danego gracza.
* `POST /fg_addons/api/api.php?action=server_quests_progress`
  * Aktualizuje postęp lub informuje o zakończeniu zadania (wysyła JSON z `steamId`, `questId`, `progress`, `completed`).
* `POST /fg_addons/api/api.php?action=server_link`
  * Weryfikuje token wpisany przez gracza na serwerze i przypisuje konto Steam.

## 5. Zdarzenia w Grze (Eventy) do Śledzenia
Wtyczka będzie wykorzystywać zdarzenia z silnika CS2 (CounterStrikeSharp) do monitorowania postępu:
* `player_death` - do liczenia zabić, headshotów, użytej broni.
* `player_hurt` - do liczenia zadanych obrażeń.
* `bomb_planted` / `bomb_defused` - do zadań związanych z celem mapy.
* `round_mvp` - do zadań za zdobycie MVP.

## 6. Architektura Techniczna
* **Framework**: CounterStrikeSharp (.NET 8.0 / C#)
* **Konfiguracja**: Plik `quests.json` zawierający URL do API, klucz API serwera (Authorization) oraz ustawienia wiadomości na czacie.
* **Komunikacja API**: `HttpClient` (asynchroniczne zapytania non-blocking, aby nie lagować serwera). Z użyciem biblioteki `System.Text.Json`.

## 7. Plan implementacji (Kroki do wykonania)
1. Inicjalizacja projektu CounterStrikeSharp (`dotnet new classlib`).
2. Utworzenie modeli danych (Player, Quest, Config).
3. Implementacja warstwy sieciowej (`ApiService`) do komunikacji z API.
4. Obsługa komend graczy (`!quests`, `!link`).
5. Rejestracja zdarzeń gry (Event Handlers) do zliczania akcji (zabójstwa, itp.).
6. Logika zarządzania postępami zadań w pamięci w czasie trwania mapy i wysyłania ich do API.
7. Dodanie powiadomień na czacie / HUD (PrintToChat / CenterHtml).
