using System;
using System.Collections.Generic;

namespace Pract2_CBT
{
    class Backtracking
    {
        //parameters voor de grootte van de sudoku
        public static int grootte = 9; //voor 9x9 blok kies 9, voor 16x16 kies 16, voor 25x25 kies 25 etc.
        public static int grootteblok = 3; //bij 9 hoort 3, bij 16 4, bij 25 5 etc.

        public static void Main(string[] args)
        {
            var watch = new System.Diagnostics.Stopwatch(); //om de executietijd te berekenen

            (int[,] sudoku, string keuze) = sudokumaken(); //sudoku wordt aangemaakt mbv de input

            watch.Start();

            //bool[,] fixate = new bool[grootte, grootte]; //nodig voor het backtracken bij forward checking

            if (keuze == "b") //dan is er gekozen voor backtracken zonder forward checking
            {
                (bool jaofnee, int[,] sud) = cbt(sudoku); // hier roep je backtracking aan
                if (jaofnee)
                {
                    watch.Stop();
                    Console.WriteLine($"Execution Time: {watch.ElapsedMilliseconds} ms");

                    Oplossing(sud); //de oplossing naar console schrijven
                }
                else
                {
                    Console.WriteLine("Er is geen oplossing voor deze sudoku");
                }
            }
            else if (keuze == "f")
            {
                //parallelle grid aanmaken om domeinen bij te houden, per vakje is er een lijst met daarin het domein van dat vakje
                List<int>[,] domeinen = new List<int>[grootte, grootte];

                for (int i = 0; i < grootte; i++)
                {
                    for (int j = 0; j < grootte; j++)
                    {
                        List<int> temp = new List<int>(); //ook nog invullen op begin namelijk allemaal 1 tm 9
                        for(int index = 1; index < (grootte + 1); index++)
                        {
                            temp.Add(index);
                        }
                        domeinen[i, j] = temp;
                    }
                }

                List<int>[,] aangepastdomein = domeinen; //de methode domeinenaanpassen maakt een nieuwe array van lists aan
                bool[,] gewijzigddomein = new bool[grootte, grootte]; //er is ook een boolean array aangemaakt die bijhoudt per aanpassen van een domein, welke domeinen er daadwerkelijk aangepast zijn.
                //Hiermee kan je dit later weer terugdraaien als het blijkt dat er een fout getal is geplaatst

                //nu al domeinen aanpassen adhv de gefixeerde getallen
                for (int r = 0; r < grootte; r++)
                {
                    for (int k = 0; k < grootte; k++)
                    {
                        if (sudoku[r, k] != 0) //dus is het gefixeerd en moeten er domeinen aanpast worden
                        {
                            int teverwijderen = sudoku[r, k];
                            (aangepastdomein, gewijzigddomein) = Domeinenaanpassen(sudoku, r, k, teverwijderen, aangepastdomein);
                            //je geeft aan domeinenaanpassen de rij en kolom mee waarin het desbetreffende getal verwijderd moet worden
                        }
                    }
                }

                (bool jaofnee, int[,] sud, List<int>[,] dom) = Cbtfc(sudoku, aangepastdomein); // hier roep je backtracking met forward checking aan
                if (jaofnee)
                {
                    watch.Stop();
                    Console.WriteLine($"Execution Time: {watch.ElapsedMilliseconds} ms");

                    Oplossing(sud);
                }
                else
                {
                    Console.WriteLine("Er is geen oplossing voor deze sudoku");
                }
            }
            else
            {
                Console.WriteLine("Er is niet een juiste keuze gemaakt voor een algoritme, kies 'b' of 'f'"); //een error voor als men de verkeerde input geeft
            }

            Console.ReadLine();
        }


        public static (int[,], string) sudokumaken()
        {
            string gridd = Console.ReadLine();            //input wordt gelezen
            string[] grid = gridd.Split(' ');
            string keuze = grid[0].ToString();           //voor welk algoritme er gebruikt wordt --> KIES: b voor cbt of f voor cbt met forward checking
            int teller = 1;                              //h oudt index van grid bij in de dubbele for loop
            int[,] sudoku = new int[grootte, grootte];               //de sudoku wordt geinitialiseerd

            for (int i = 0; i < grootte; i++)
            {
                for (int j = 0; j < grootte; j++)
                {
                    string va = grid[teller].ToString(); //om het vervolgens te kunnen parsen naar een integer en het in de 2d array te kunnen zetten
                    int element = int.Parse(va);
                    sudoku[i, j] = element;
                    teller++;
                }
            }

            return (sudoku, keuze); //zodat je aan de slag kan met de sudoku met gefixeerde getallen en de keuze voor het algoritme
        }

        public static void Oplossing(int[,] sudoku)       //met deze methode wordt de oplossing naar de console geschreven
        {
            for (int i = 0; i < grootte; i++)
            {
                for (int j = 0; j < grootte; j++)
                {
                    Console.Write("{0} ", sudoku[i, j]);
                }

                Console.Write(Environment.NewLine + Environment.NewLine);
            }
        }

        public static (bool, int[,]) cbt(int[,] sudoku) //recursieve methode voor chronological backtracking
        {
            int rij = -1; int kolom = -1; bool gefixeerd = true; //initialisatie
            for (int i = 0; i < grootte; i++)
            {
                for (int j = 0; j < grootte; j++)
                {
                    if (sudoku[i, j] == 0) //we checken eerst of het element al een waarde heeft, als dat nog niet zo is dan:
                    {
                        rij = i; kolom = j; gefixeerd = false; //dan veranderen we de variabelen rij en kolom zodat de index van het lege element onthouden wordt
                        break;
                    }
                }
                if (gefixeerd == false) //als deze lijn code wordt bereikt omdat de vorige if statement is getriggered gaan we uit de for loop met de coordinaten van het lege element
                {
                    break;
                }
            }

            if (gefixeerd) //als deze lijn code wordt bereikt zijn er geen lege elementen meer, oftewel de for loop is afgelopen zonder dat de eerste if statement is getriggerd en dat duidt er op dat de gehele sudoku is ingevuld
            {
                return (true, sudoku);
            }

            for (int x = 1; x < (grootte + 1); x++) //ga alle getallen langs die ingevuld kunnen worden van klein naar groot
            {
                if (Check(sudoku, rij, kolom, x)) //de methode check wordt aangeroepen om te kijken of x een geldige waarde is voor het huidige vakje (obv rij/kolom/3x3vak)
                {
                    sudoku[rij, kolom] = x; //dit is wat je verandert aan de sudoku, dus dit moet teruggekoppeld worden
                    (bool jaofnee, int[,] sud) = cbt(sudoku); //hier treedt de methode in recursie, met de aangepaste sudoku
                    if (jaofnee) //als daaruit blijkt dat er iedere keer een geldige sudoku kan ontstaan, kan je het helemaal ingevuld returnen
                    {
                        return (true, sudoku); //als uit de recursie true komt kan je zeggen dat er een oplossing is gevonden
                    }
                    else
                    {
                        sudoku[rij, kolom] = 0; //als uit de recursie false komt moeten we het opnieuw proberen, dus we zetten de waarde weer naar 0 en gaan door kijken bij de volgende waarde van x
                    }
                }
            }

            return (false, sudoku); //als we geen true hebben kunnen returnen en dus deze lijn code bereiken blijkt het dat er geen oplossing te vinden is.
        }


        public static bool Check(int[,] sudoku, int rij, int kolom, int x) //methode voor cbt
        {
            int vierkantRij = rij - rij % grootteblok; //dit is een formule om te achterhalen in welk 3x3 blok je zit obv de rij waar je je in bevindt
            int vierkantKolom = kolom - kolom % grootteblok;

            for (int a = vierkantRij; a < vierkantRij + grootteblok; a++) //in deze loops checken we of het getal dat we willen invoeren al in dit 3x3 blok zit
            {
                for (int b = vierkantKolom; b < vierkantKolom + grootteblok; b++)
                {
                    if (sudoku[a, b] == x)
                    {
                        return false; //als het getal dus al in dit 3x3 blok zit returnen we false, dan mag je namelijk niet ook nog dat getal ergens anders in het blok zetten
                    }
                }
            }

            for (int i = 0; i < grootte; i++)   //in deze for loop checken we of het getal wat we willen invoeren al in deze rij staat
            {
                if (sudoku[rij, i] == x)
                {
                    return false;         //als het getal dus al in deze rij staat returnen we false
                }
            }
            for (int j = 0; j < grootte; j++)   //in deze for loop checken we of het getal wat we willen invoeren al in deze kolom staat
            {
                if (sudoku[j, kolom] == x)
                {
                    return false;         //als het getal dus al in deze kolom staat returnen we false
                }
            }
            return true;
        }

        public static (bool, int[,], List<int>[,]) Cbtfc(int[,] sudoku, List<int>[,] domeinen) //recursieve methode voor cbt met forward cheching
        {
            //domein ook iedere keer returnen zodat het de hele tijd aangepast doorgegeven wordt
            int rij = -1; int kolom = -1; bool gefixeerd = true; //begin is hetzelfde als cbt
            for (int i = 0; i < grootte; i++)
            {
                for (int j = 0; j < grootte; j++)
                {
                    if (sudoku[i, j] == 0)                    //we checken eerst of het element al een waarde heeft
                    {
                        rij = i; kolom = j; gefixeerd = false;    //zo niet, dan veranderen we de variabelen rij en kolom zodat de index van het lege element onthouden wordt
                        break;
                    }
                }
                if (gefixeerd == false)                           //als deze lijn code wordt bereikt omdat de vorige if statement is getriggered gaan we uit de for loop
                {
                    break;
                }
            }

            if (gefixeerd)            //als deze lijn code wordt bereikt zijn er geen lege elementen meer, oftewel de for loop is afgelopen zonder dat de eerste if statement is getriggerd
            {
                return (true, sudoku, domeinen);
            }

            if (domeinen[rij, kolom].Count != 0) //hier checken of er niet een leeg domein is, als dat wel zo is false returnen want dan zijn er geen mogelijke getallen in te vullen en moet je backtracken
            {
                foreach (int x in domeinen[rij, kolom].ToArray()) //elke mogelijke x proberen die in het domein zit, te beginnen bij het laagste getal (dit zijn dus alleen maar getallen die al geldig zijn)
                {
                    sudoku[rij, kolom] = x; //dan verander je de sudoku
                    (List<int>[,] aangepastdomein, bool[,] gewijzigddomein) = Domeinenaanpassen(sudoku, rij, kolom, x, domeinen); //en ga je vervolgens forward checken
                    (bool jaofnee, int[,] sud, List<int>[,] dom) = Cbtfc(sudoku, aangepastdomein); //hier treedt de methode in recursie, met het nieuwe domein meegegegeven
                    if (jaofnee)
                    {
                        return (true, sud, dom); //als uit de recursie true komt kan je zeggen dat er een oplossing is gevonden 
                    }
                    else
                    {
                        sudoku[rij, kolom] = 0;     //als uit de recursie false komt moeten we het opnieuw proberen, dus we zetten de waarde weer naar 0

                        for (int br = 0; br < grootte; br++)   //hele boolean array doorlopen, als er iets is gewijzigd, wordt dat hier weer toegevoegd aan het domein (terugdraaien)
                        {
                            for (int bk = 0; bk < grootte; bk++)
                            {
                                if (gewijzigddomein[br, bk]) //dan was er iets gewijzigd hier, dus moet het nu teruggezet worden
                                {
                                    if (!domeinen[br, bk].Contains(x)) //wel nog even checken of het niet al in het domein zit (tegen errors)
                                    {
                                        domeinen[br, bk].Add(x); //weer terugzetten 
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return (false, sudoku, domeinen); // als we geen true hebben kunnen returnen en dus deze lijn code bereiken blijkt het dat er geen oplossing te vinden is.
        }

        public static (List<int>[,], bool[,]) Domeinenaanpassen(int[,] sudoku, int rij, int kolom, int teverwijderen, List<int>[,] dom) //dit is de forward check
        {
            bool[,] gewijzigddomein = new bool[grootte, grootte]; //boolean array om bij te houden of iets in een domein gewijzigd wordt, als dat zo is, moet het (indien het later fout blijkt te zijn) terug gezet kunnen worden

            for (int br = 0; br < grootte; br++)   //hele boolean array initieren op false 
            {
                for (int bk = 0; bk < grootte; bk++)
                {
                    gewijzigddomein[br, bk] = false;
                }
            }

            for (int i = 0; i < grootte; i++) //loop om alle lijsten in de kolom van x bij te werken
            {
                if (i != kolom) //het getal mag wel in het domein blijven van het coordinaat zelf
                {
                    if (dom[rij, i].Contains(teverwijderen)) //om errors van empty dingen removen te verkomen
                    {
                        dom[rij, i].Remove(teverwijderen);
                        gewijzigddomein[rij, i] = true; //als er iets gewijzigd is, op true zetten
                    }
                }
            }

            for (int j = 0; j < grootte; j++) //loop om alle lijsten in de rij van x bij te werken
            {
                if (j != rij) //het getal mag wel in het domein blijven van het coordinaat zelf
                {
                    if (dom[j, kolom].Contains(teverwijderen))
                    {
                        dom[j, kolom].Remove(teverwijderen);
                        gewijzigddomein[j, kolom] = true; //als er iets gewijzigd is, op true zetten
                    }
                }
            }

            //ook nog alles in 3x3 blok verwijderen
            int vierkantRij = rij - rij % grootteblok;        //dit is een formule om te achterhalen in welk 3x3 blok je zit obv de rij waar je je in bevindt
            int vierkantKolom = kolom - kolom % grootteblok;

            for (int k = vierkantRij; k < (vierkantRij + grootteblok); k++)
            {
                for (int l = vierkantKolom; l < (vierkantKolom + grootteblok); l++)
                {
                    if (k != rij && l != kolom) //het getal mag wel in het domein blijven van het coordinaat zelf
                    {
                        if (dom[k, l].Contains(teverwijderen))
                        {
                            dom[k, l].Remove(teverwijderen);
                            gewijzigddomein[k, l] = true; //als er iets gewijzigd is, op true zetten
                        }
                    }
                }
            }
            return (dom, gewijzigddomein); //geef het gewijzigde domein mee terug en de boolean array van welke domeinen daadwerkelijk zijn aangepast
        }
    }
}