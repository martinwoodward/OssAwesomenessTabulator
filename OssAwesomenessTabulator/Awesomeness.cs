using OssAwesomenessTabulator.Data;
using System;

namespace OssAwesomenessTabulator
{
    /// <summary>
    ///  Awesomeness was inspired by Twitter's Hotness algo but uses more Pi because
    ///  Pi is always awesome.
    ///
    ///    https://github.com/twitter/twitter.github.com/blob/289a70a500a478cd039bb9994a48e58cffe2bc61/index.html#L76-L85
    ///
    ///  But really just an excuse for me to use some of the maths I did in my degree
    ///  with a bit of exponential decay and an sprinkling of irrational numbers
    /// </summary>
    public static class Awesomeness
    {

        public static readonly double Push_Awesomeness = 1000;
        public static readonly double Push_Halflife = 42 * Math.E * Math.Pow(10, -15);
        public static readonly double Star_Awesomeness = (10 + Math.PI) * Math.Pow(10, 13);

        public static int Calculate(Project project)
        {
            if (project.CommitLast == null)
            {
                // Project hasn't contributed any code yet, not very awesome
                return 0;
            }

            double pushTicks = DateTimeOffset.Now.Subtract((DateTimeOffset)project.CommitLast).Ticks;
            double createdTicks = DateTimeOffset.Now.Subtract(project.Created).Ticks;

            // People power, if you have a high star factor (i.e. stars per day) then you
            // are definately awesome.
            double awesomeness = (Star_Awesomeness * project.Stars) / createdTicks;
            
            // Make it so a recent contribution pushes you up the stack, but make the effect
            // fade quickly (as determined by the halflife of a push.
            awesomeness += Push_Awesomeness * Math.Pow(Math.E, -1 * Push_Halflife * pushTicks);

            // Forks can be awesome, but generally the root project is more awesome and there
            // is always a tax to pay when forking. In this instance it's 20%
            if (project.IsFork)
            {
                awesomeness = awesomeness * 0.8;
            }
            
            // Everyone who makes their code open source is a little bit awesome.
            awesomeness++;

            if (awesomeness > Int16.MaxValue)
            {
                // Because nobody likes a show-off.
                awesomeness = Int16.MaxValue;
            }

            return (int)awesomeness;
        }
    }
}
