using Raylib_cs;
using System.Numerics;

// Initialization

Raylib.InitWindow(960, 720, "Topdown Test");
Raylib.SetTargetFPS(60);

// Variables

string scene = "menu";
bool buttonsEnabled = true;

// Textures

Texture2D background = Raylib.LoadTexture(@"resources/MainMenu.png");
Texture2D menuButton = Raylib.LoadTexture(@"resources/Button.png");
Texture2D hero = Raylib.LoadTexture(@"resources/Spert.png");

// Functions

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

// Loop when raylib not closed

while (!Raylib.WindowShouldClose())
{
    // Initialization of drawing
    
    Raylib.BeginDrawing();
    Raylib.ClearBackground(Color.WHITE);

    if (scene == "menu")
    {
        Raylib.DrawTexture(background, 0, 0, Color.WHITE);
        createButton("Play", 512, 192, openGame);
        createButton("Settings", 512, 320, settings);
    }
    else if (scene == "game")
    {
        Raylib.DrawTexture(hero, 0, 0, Color.WHITE);
    }

    Raylib.EndDrawing();
}