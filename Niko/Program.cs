using Raylib_cs;
using System.Numerics;
using System.Linq;

// Initialisation

Raylib.InitWindow(960, 720, "Topdown Test");
Raylib.SetTargetFPS(60);

// Variabler

string scene = "menu";
bool buttonsEnabled = true;

float xDir = 0;
float yDir = 0;

Vector2 playerPos = new Vector2((float)448, (float)328);

List<Vector2> speedOrbPositions = new();
IDictionary<Vector2, int> specialOrbPositions = new Dictionary<Vector2, int>();
IDictionary<Vector2, bool> enemyOrbPositions = new Dictionary<Vector2, bool>();
IDictionary<int, Vector2> enemyOrbLocalPositions = new Dictionary<int, Vector2>();
IDictionary<int, float> enemyOrbSpeed = new Dictionary<int, float>();

List<string> messages = new();

bool speedCooldown = false;
bool specialCooldown = false;
bool enemyCooldown = false;
bool dead = false;
bool exitButtonPressed = false;
bool debugMode = false;

float speed = 3;
float smooth = 0.25f;

int timeElapsed = 0;
int points = 0;

// Bilder

Texture2D background = Raylib.LoadTexture(@"resources/MainMenu.png");
Texture2D menuButton = Raylib.LoadTexture(@"resources/Button.png");
Texture2D hero = Raylib.LoadTexture(@"resources/Spert.png");

// Collisioner

Rectangle heroPhys = new Rectangle(playerPos.X, playerPos.Y, 32, 32);

// Funktioner

void createButton(string text, int x, int y, Action a)
{
    Color color = Color.LIGHTGRAY;

    if(getZone(x, y, 392, 96))
    {
        color = Color.WHITE;
    }

    Raylib.DrawTexture(menuButton, x, y, color);

    int xMid = (int)Raylib.MeasureTextEx(Raylib.GetFontDefault(), text, 64, 0).X / 2;
    int yMid = (int)Raylib.MeasureTextEx(Raylib.GetFontDefault(), text, 64, 0).Y / 2;

    Raylib.DrawText(text, x + 192 - xMid, y + 48 - yMid, 64, Color.DARKGRAY);

    if(getZone(x, y, 392, 96) && Raylib.IsMouseButtonPressed(MouseButton.MOUSE_BUTTON_LEFT) && buttonsEnabled)
    {
        Task t = new Task(a);
        t.Start();
    }
    
}

bool getZone(int x, int y, int width, int height)
{
    int xStart = x;
    int xEnd = x + width;
    
    int yStart = y;
    int yEnd = y + height;

    if(Raylib.GetMousePosition().X >= xStart && Raylib.GetMousePosition().X <= xEnd && Raylib.GetMousePosition().Y >= yStart && Raylib.GetMousePosition().Y <= yEnd) 
    {
        return true; 
    }
    else 
    {
        return false;
    }
}

void openGame()
{
    scene = "game";
    buttonsEnabled = false;
}

void settings()
{
    Console.WriteLine("To be implemented");
}

int getAxis(string direction)
{
    if (direction == "Horizontal")
    {
        if (Raylib.IsKeyDown(KeyboardKey.KEY_D)) return 1;
        if (Raylib.IsKeyDown(KeyboardKey.KEY_A)) return -1;
    }
    else if (direction == "Vertical")
    {
        if (Raylib.IsKeyDown(KeyboardKey.KEY_S)) return 1;
        if (Raylib.IsKeyDown(KeyboardKey.KEY_W)) return -1;
    }
    return 0;
}

float smoothStep(float min, float max, float step, bool negative)
{
    if (negative == true)
    {
        return min - (MathF.Abs(min - max) * step);
    }
    return min + (MathF.Abs(min - max) * step);
}

bool countingDown = false;
async void timeCount()
{
    if (countingDown) return;
    countingDown = true;
    await Task.Delay(1000);
    timeElapsed++;
    points++;
    countingDown = false;
}

// Ljud

Raylib.InitAudioDevice();
Sound menuMusic = Raylib.LoadSound(@"resources/music/ageispolis.ogg");
Sound gameMusic = Raylib.LoadSound(@"resources/music/pulsewidth.ogg");

// Loopa när raylib är inte stängd

while (!Raylib.WindowShouldClose())
{
    // Börja rita
    
    Raylib.BeginDrawing();
    Raylib.ClearBackground(Color.WHITE);

    if (scene == "menu")
    {
        // Stoppa alla musik och Spela Musik

        Raylib.StopSound(gameMusic);
        if (!Raylib.IsSoundPlaying(menuMusic)) Raylib.PlaySound(menuMusic);

        // Visa bakgrund och knappor

        Raylib.DrawTexture(background, 0, 0, Color.WHITE);
        createButton("Play", 512, 192, openGame);
        createButton("Settings", 512, 320, settings);

        void exitGame()
        {
            exitButtonPressed = true;
        }

        // Verifera att ingenting är fel

        enemyOrbPositions.Clear();
        speedOrbPositions.Clear();
        enemyOrbSpeed.Clear();
        enemyOrbLocalPositions.Clear();

        createButton("Exit", 512, 448, exitGame);

        if (exitButtonPressed == true)
        {
            Raylib.CloseAudioDevice();
            Raylib.CloseWindow();
            break;
        }
    }
    else if (scene == "game")
    {
        // Spela Musik

        Raylib.StopSound(menuMusic);
        if (!Raylib.IsSoundPlaying(gameMusic)) Raylib.PlaySound(gameMusic);

        // Hjälten ska leva (bara ifall de är döda)

        dead = false;

        // Variabler
        
        bool isXNegative = false;
        bool isYNegative = false;

        // Titta om axiserna är negativt eller inte

        if (getAxis("Horizontal") < 0) isXNegative = true;
        if (getAxis("Vertical") < 0) isYNegative = true;

        // Rörelse

        if (getAxis("Horizontal") != 0) xDir = smoothStep(xDir, getAxis("Horizontal"), smooth, isXNegative);
        else xDir *= 1 - smooth;

        if (getAxis("Vertical") != 0) yDir = smoothStep(yDir, getAxis("Vertical"), smooth, isYNegative);
        else yDir *= 1 - smooth;

        void updateMovement()
        {
            Vector2 movement = new Vector2(xDir, yDir);

            if(movement.Length() > 1f) movement = Vector2.Normalize(movement);

            playerPos += movement * speed;

            playerPos.X = Math.Clamp(playerPos.X, 0, 896);
            playerPos.Y = Math.Clamp(playerPos.Y, 0, 656);

            heroPhys.x = playerPos.X;
            heroPhys.y = playerPos.Y;
        }

        updateMovement();

        // Rita hjälten & Text

        Raylib.DrawTextureV(hero, playerPos, Color.WHITE);
        Raylib.DrawText($"SPEED: {MathF.Round(speed, 2)}", 8, 8, 32, Color.GREEN);
        timeCount();
        Raylib.DrawText($"POINTS: {points}", 8, 36, 32, Color.GREEN);

        // Functioner för att göra entiteter
        // Fabricate: Addera entitets position på ett lista för entiteter som ska ritas
        // Spawn: Gör ett position för entiteten, vänta, och sen tillverka till listan.

        void fabricateSpeedOrb(Vector2 pos)
        {
            speedOrbPositions.Add(pos);
        }

        async void spawnSpeedOrb()
        {
            Random rnd = new Random();

            Vector2 speedPos = new Vector2(rnd.Next(0, 928), rnd.Next(0, 688));
            speedCooldown = true;

            await Task.Delay(5000);

            fabricateSpeedOrb(speedPos);
            speedCooldown = false;
        }

        void fabricateSpecialOrb(Vector2 pos)
        {
            Random rnd = new Random();

            specialOrbPositions.Add(pos, rnd.Next(1, 5));
        }

        async void spawnSpecialOrb()
        {
            Random rnd = new Random();

            Vector2 specialPos = new Vector2(rnd.Next(0, 928), rnd.Next(0, 688));
            specialCooldown = true;

            await Task.Delay(rnd.Next(15000, 60000));

            fabricateSpecialOrb(specialPos);
            specialCooldown = false;
        }

        async void fabricateEnemyOrb(Vector2 pos)
        {
            enemyOrbPositions.Add(new KeyValuePair<Vector2, bool>(pos, true));
            await Task.Delay(1000);
            enemyOrbPositions.Remove(pos);

            enemyOrbPositions.Add(new KeyValuePair<Vector2, bool>(pos, false));
        }

        async void spawnEnemyOrb()
        {
            Random rnd = new Random();

            Vector2 enemyPos = new Vector2(rnd.Next(0, 896), rnd.Next(0, 656));
            enemyCooldown = true;

            await Task.Delay(10000);

            fabricateEnemyOrb(enemyPos);
            enemyCooldown = false;
        }

        // Göra saker
        // Om cooldown är klart och hjälten är inte död så skapas entiteten.

        // For loop:
        // För varje entitets position på listan ritas entiteten.
        // Om hjälten och entiteten colliderar med varandra så görs effekten.

        if (speedCooldown == false && dead == false) spawnSpeedOrb();

        for (int i = 0; i < speedOrbPositions.Count(); i++)
        {
            Raylib.DrawCircleV(speedOrbPositions[i], 16, Color.GREEN);

            if (Raylib.CheckCollisionCircleRec(speedOrbPositions[i], 16, heroPhys))
            {
                speedOrbPositions.Remove(speedOrbPositions[i]);
                speed *= 1.05f;
                points+= 5;
            }
        }

        if (specialCooldown == false && dead == false) spawnSpecialOrb();

        bool textCooldown = false;
        foreach (KeyValuePair<Vector2, int> element in specialOrbPositions)
        {
            Raylib.DrawCircleV(element.Key, 12, Color.SKYBLUE);

            if (Raylib.CheckCollisionCircleRec(element.Key, 12, heroPhys))
            {
                specialOrbPositions.Remove(element.Key);
                
                if (element.Value == 1)
                {
                    messages.Add("Double Speed");
                    speed *= 2;
                }
                else if (element.Value == 2)
                {
                    messages.Add("Speed Reset");
                    speed = 3;
                }
                else if (element.Value == 3)
                {
                    messages.Add("Enemies Gone");
                    enemyOrbPositions.Clear();
                    enemyOrbSpeed.Clear();
                    enemyOrbLocalPositions.Clear();
                }
                else if (element.Value == 4)
                {
                    messages.Add("Enemies Double Speed");
                    foreach(KeyValuePair<int, float> speedie in enemyOrbSpeed)
                    {
                        enemyOrbSpeed[speedie.Key] *= 2;
                    }
                }
                else
                {
                    messages.Add("NOTHING");
                }
            }
        }

        if (enemyCooldown == false && dead == false) spawnEnemyOrb();

        int enemyIndex = 0;
        foreach (KeyValuePair<Vector2, bool> element in enemyOrbPositions)
        {
            enemyIndex++;
            Vector2 enemyPos = new Vector2();
            if (element.Value == true){
                enemyPos = element.Key;
                Raylib.DrawCircleV(element.Key, 32, Color.RED);
            }
            else {
                if (!enemyOrbLocalPositions.ContainsKey(enemyIndex)) enemyOrbLocalPositions.Add(new KeyValuePair<int, Vector2>(enemyIndex, element.Key));
                enemyPos = enemyOrbLocalPositions[enemyIndex];
                Vector2 enemyDir = Vector2.Normalize(((playerPos + new Vector2(32, 32)) - enemyPos) / ((playerPos + new Vector2(32, 32)) - enemyPos).Length());

                if (!enemyOrbSpeed.ContainsKey(enemyIndex))
                {
                    Random enemyRnd = new Random();
                    float enemyOrbSpeedVar = enemyRnd.Next(50, 300);
                    enemyOrbSpeedVar /= 100;
                    enemyOrbSpeed.Add(new KeyValuePair<int, float>(enemyIndex, enemyOrbSpeedVar));
                }

                enemyPos += enemyDir * enemyOrbSpeed[enemyIndex] * 0.75f;
                enemyOrbLocalPositions[enemyIndex] = enemyPos;
                Raylib.DrawCircleV(enemyPos, 32, Color.RED);
            }

            if (Raylib.CheckCollisionCircleRec(enemyPos, 32, heroPhys) && element.Value == false)
            {
                dead = true;
                break;
            }
        }

        int textIndex = 0;
        foreach(string text in messages)
        {
            async void textTimer()
            {
                await Task.Delay(5000);
                messages.Remove(text);
            }
            Raylib.DrawText($"ABILITY: {text}", 8, 72 + (36*textIndex), 32, Color.SKYBLUE);
            if (textCooldown == false)
            {
                textCooldown = true;
                textTimer();
            }
            textIndex++;
        }
    }

    // Om hjälten är död i spelen, gå tillbaka till spelet.

    if (dead && scene == "game")
    {
        enemyOrbPositions.Clear();
        speedOrbPositions.Clear();
        enemyOrbSpeed.Clear();
        enemyOrbLocalPositions.Clear();

        playerPos = new Vector2((float)448, (float)328);
        xDir = 0;
        yDir = 0;
        timeElapsed = 0;
        Console.WriteLine($"YOU GOT {points} POINTS");
        points = 0;
        speed = 3;

        scene = "menu";
        buttonsEnabled = true;
    }

    // Titta om vi går på debug eller inte
    if(Raylib.IsKeyPressed(KeyboardKey.KEY_F3))
    {
        debugMode = !debugMode;
    }

    //Rita Debug
    if(debugMode == true)
    {
        Raylib.DrawText($"FPS: {Raylib.GetFPS()}", 8, 684, 32, Color.RED);
        Raylib.DrawText($"DIRECTION: {MathF.Round(xDir, 2)}, {MathF.Round(yDir, 2)}", 8, 648, 32, Color.RED);
        Raylib.DrawText($"TIME: {MathF.Round(xDir, 2)}, {MathF.Round(yDir, 2)}", 8, 612, 32, Color.RED);
    }

    Raylib.EndDrawing();
}