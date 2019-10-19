import java.util.Random;
import java.util.Scanner;

public class Program {
    public void main(String[] args) {
        System.out.println("Loading...");
        Scanner scanner = new Scanner(System.in);
        Random random = new Random();
        System.out.println("Welcome to guessing game");
        boolean exit = false;
        while(!exit)
        {
            System.out.println("Lower bound:");
            int low = scanner.nextInt();
            System.out.println("Upper bound:");
            int high = scanner.nextInt();
            if(low < high)
            {
                int number = random.nextInt(high - low) + low;
                int guess = number + 1;
                while(guess != number)
                {
                    System.out.println("Guess:");
                    guess = scanner.nextInt();
                    if(guess < number)
                    {
                        System.out.println("Too low");
                    }
                    else if(guess > number)
                    {
                        System.out.println("Too high");
                    }
                    else
                    {
                        System.out.println("Correct");
                    }
                }
                System.out.println("Type 'c' to continue");
                exit = scanner.next() != "c"; //doesn't work
            }
            else
            {
                System.out.println("Invalid inputs");
            }
        }
    }
}