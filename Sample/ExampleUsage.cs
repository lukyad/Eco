using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Eco;
using Eco.Converters;

namespace Sample
{
    /// <summary>
    /// The root type of our configuration schema.
    /// The ParsingPolicy specified below will be applied recoursive to all fields of type TimeSpan.
    /// </summary>
    [Root, Doc("Represent one round of a space battle.")]
    [ParsingPolicy(typeof(TimeSpanConverter))]
    public class spaceBattle
    {
        /// <summary>
        /// The 'variable' element type is provided by the Eco library
        /// and has a special processing rules. 
        /// Basically, any variables defined in this section
        /// can be referenced any where in the configuration file using ${varName} syntax.
        /// The Eco library will expand all variables when reading a config file.
        /// </summary>
        [Doc("Configuration variables.")]
        public variable[] vars;

        /// <summary>
        /// If field is marked with the Required attribute,
        /// it must be present in the configuration file.
        /// Otherwise, an exception will be thrown.
        /// 
        /// Rename attribute instructs serializer to use a new name for all elements of the array.
        /// Thus, elements of the 'gameRounds' array will be renamed to 'add' in place of default 'gameRound'.
        /// </summary>
        [Required, Rename(".+", "add")]
        [Doc("Battles specification.")]
        public gameRound[] gameRounds;

        [Required, Doc("Fleets that can participate in the battle.")]
        public fleet[] fleets;

        /// <summary>
        /// Here we instuct the Eco library to load this element from an external file.
        /// In the config file externalFlees field is replaced with an element of type 'Eco.Elemens.include'.
        /// </summary>
        [Doc("Fleets defined in a external configuration file.")]
        public include<externalFleets> externalFleets;
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
        /// <summary>
        /// Ref indicates that humanFleet element is defined somewhere else in the configuration file
        /// and here we reference it by it's name. (fleet type must have a field of type string marked with the Id attribute)
        /// </summary>
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
        /// <summary>
        /// fleet can be referenced anywhere in the configuration file by it's name.
        /// </summary>
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

        /// <summary>
        /// We use KnownTypes attribute to specifiy schema types that can occure here.
        /// </summary>
        [KnownTypes(wildcard:"*Weapon", context: typeof(weapon)), Rename("sample(.+)Weapon", "$1")]
        [Doc("References to weapons to be used by the ship in a combat.")]
        public object[] weapons;

        /// <summary>
        /// 'drive' is an abstract base class. All derived classes are automatically becomes known to serializer.
        /// </summary>
        [Required, Doc("Drive affects mobility of the ship during a combat.")]
        public driveChoice drive;
    }

    public class driveChoice
    {
        [Required]
        public drive choice;
    }

    public abstract class drive    { }

    public abstract class weapon { }

    [Doc("Deals 3 - 8 damage to the target.")]
    public class sampleIonCannonWeapon : weapon
    {
    }

    [Doc("Deals 4 - 16 damage with a 1 space range.")]
    public class sampleFusionBeamWeapon : weapon
    {
    }

    [Doc("Regular is 5 - 20. Heavy is 5 - 40 with a 2 space range.")]
    public class samplePhaserWeapon : weapon
    {
    }

    [Doc("10 - 40 damage, Range 2.")]
    public class sampleDisruptorWeapon : weapon
    {
    }

    [Doc("Deals 6 damage to the target. Adds +1 level to the attacker's attack rating for this missile only.")]
    public class sampleHyperRocketWeapon : weapon
    {
    }

    [Doc("Deals 15 damage to the target. Adds + 3 level to the attacker's attack rating for this missile only.")]
    public class sampleStingerMissleWeapon : weapon
    {
    }

    [Doc("Deals 30 damage, can only be fired every other turn.")]
    public class sampleAntiMatterTorpedoesWeapon : weapon
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
