import java.util.Scanner;

import java.net.*;
import java.io.*;

public class Program {
    public static native void ToggleDebugWrite(boolean value);
    public void main(String[] args) throws IOException{
        System.out.println("Loading...");
        ToggleDebugWrite(true);
        GenericClass<Integer> holder = new GenericClass<Integer>(2);
        int obj = holder.GetObject();
        ToggleDebugWrite(false);
        System.out.println(obj);
    }
}