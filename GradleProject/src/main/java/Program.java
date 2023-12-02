import java.util.Scanner;
public class Program {
    public String thingy() {
        String a = "x";
        try {
            int b = 0/0;
        }
        catch (Exception e) {
            return a;
        }
        
        return a + "y";
    }
    public void main(String[] args) {
        /*System.out.println("Hello, World!");
        Scanner scanner = new Scanner(System.in);
        String a = scanner.next();*/
        thingy();
    }
}