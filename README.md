# meteostanice
Aplikace se spouští z konzole a vyžaduje jeden parametr - vstupní URL adresu s XML souborem.
Po spuštění aplikace přečte obsah XML na uvedené URL adrese, převede ho do formátu JSON a vloží do databázového souboru - ten musí být umístěn ve stejném adresáři jako spouštěcí EXE soubor.
Vyvíjení probíhalo ve Visual Studiu a byly využity NuGet balíčky Microsoft.Data.SqlClient a Newtonsoft.Json.
Implementace zabrala asi cca 5 hodin.
