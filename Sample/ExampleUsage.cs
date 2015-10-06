using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Eco;
using Eco.Elements;

namespace Sample
{
    [Root, Doc("Represent one round of a space battle.")]
    [ParsingRule(typeof(TimeSpan), typeof(TimeSpanParser))]
    public class spaceBattle
    {
        [Doc("Configuration variables.")]
        public variable[] vars;

        [Required, ItemName("add")]
        [Doc("Battles specification.")]
        public gameRound[] gameRounds;

        [Required, Doc("Fleets that can participate in the battle.")]
        public fleet[] fleets;

        [External, Doc("Fleets defined in a external configuration file.")]
        public externalFleets externalFleets;
    }

    [Root, Doc("Fleets defined in a external configuration file.")]
    public class externalFleets
    {
        [Required, Inline, Doc("Fleets that can participate in a space battle.")]
        public fleet[] fleets;
    }

    [Doc("One round of a space battle.")]
    public class gameRound
    {
        [Required, Ref, Doc("Fleet that will be fighting on the human's side.")]
        public fleet humanFleet;

        [Required, Ref, Doc("Fleet that will be fighting on the alians's side.")]
        public fleet aliensFleet;

        //[Converter(typeof(TimeSpanConverter), "s")]
        public TimeSpan duration;
    }

    [Doc("Definition of a fleet that can participate in a space battle.")]
    public class fleet
    {
        [Id, Doc("Name of the fleet.")]
        public string name;

        [Doc("Spaceship's color for this fleet.")]
        public FleetColor color;

        [Inline, Doc("Spaceships of the fleet.")]
        public spaceShip[] ships;
    }

    public enum FleetColor
    {
        White,
        Green,
        Red,
        Blue
    }

    [Doc("Model of a space ship")]
    public class spaceShip
    {
        [Required, Id, Doc("Unique identifier of the space ship that can be used to reference it later on.")]
        public string name;

        [Doc("Armor protects ship from all types of weapon. Bigger is better, but more costly.")]
        public int armor;

        [Doc("Shield is twice as effective protecting ship from energy weapons comparing to armor, but doesn't protect ship from missels attacts at all.")]
        public int? shield;

        [Doc("References to weapons to be used by the ship in a combat.")]
        public weapon[] weapons;

        // Explicitly specify known types through a wildcard.
        [Required, Choice, Doc("Drive affects mobility of the ship during a combat.")]
        public drive drive;
    } 

    //public class driveChoice
    //{
    //    [Required, Polimorphic]
    //    public drive instance;
    //}

    public abstract class drive    { }

    public abstract class weapon { }

    [Doc("Deals 3 - 8 damage to the target.")]
    public class ionCannon : weapon
    {
    }

    [Doc("Deals 4 - 16 damage with a 1 space range.")]
    public class fusionBeam : weapon
    {
    }

    [Doc("Regular is 5 - 20. Heavy is 5 - 40 with a 2 space range.")]
    public class phaser : weapon
    {
    }

    [Doc("10 - 40 damage, Range 2.")]
    public class disruptor : weapon
    {
    }

    [Doc("Deals 6 damage to the target. Adds +1 level to the attacker's attack rating for this missile only.")]
    public class hyperRocket : weapon
    {
    }

    [Doc("Deals 15 damage to the target. Adds + 3 level to the attacker's attack rating for this missile only.")]
    public class stingerMissle : weapon
    {
    }

    [Doc("Deals 30 damage, can only be fired every other turn.")]
    public class antiMatterTorpedoes : weapon
    {
    }

    [Doc("Gives a maximum number of movement squares in space combat of 1.")]
    public class nuclearDrive : drive
    {
    }

    [Doc("Gives a maximum number of movement squares in space combat of 2.")]
    public class fusionDrive : drive
    {
    }

    [Doc("Gives a maximum number of movement squares in space combat of 3.")]
    public class ionDrive : drive
    {
    }

    [Doc("Gives a maximum number of movement squares in space combat of 4.")]
    public class hyperDrive : drive
    {
    }
}
