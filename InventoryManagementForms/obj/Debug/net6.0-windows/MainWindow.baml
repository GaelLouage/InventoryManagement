   M S B A M L   `   `   `  ����S  OInventoryManagementForms, Version=1.0.0.0, Culture=neutral, PublicKeyToken=nullC&clr-namespace:InventoryManagementFormsInventoryManagementForms  5   	   )    #InventoryManagementForms.MainWindow   5   	   W SPresentationCore, Version=6.0.2.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=6.0.2.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35e aSystem.Windows.Controls.Ribbon, Version=6.0.2.0, Culture=neutral, PublicKeyToken=b77a5c561934e089R NWindowsBase, Version=6.0.2.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35F 9http://schemas.microsoft.com/winfx/2006/xaml/presentation     5   	   R NSystem.Xaml, Version=6.0.2.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   8local&clr-namespace:InventoryManagementForms     5   	   -   5          � 
ResizeMode$  NoResize=�5   	   $	��400��5	   
   $	��800��6   .��5
      � ���	#FF0E314C � RowDefinitions 5   
   �� 5      $��40 �6   �� 5      $��* �6   �� 5      $��* �6   �� 5      $��* �6   �� 5      $��* �6   �� 5      $��* �6   �� 5      $��* �6   �� 5      $��* �6   �� 5      $��40 �6    � ColumnDefinitions 5   
   �� 5      $��20 �6   �� 5      $��* �6   �� 5      $��* �6   �� 5      $��* �6   �� 5      $��* �6   �� 5      $��20 �6   .+�5   
   �� $��Log In��6   $��1��6!   $��1��6.   $��2��6>   $��	5 -25 5 5q�6R   
�������$
��Bold"�6x   $��25(�6�   $	��Top=�6�   $��Center=�6�   �� 5   
   $��2��6   $��1��6   $��Username��6-   $��35 5 5 5q�6@   
�������$��Bottom=�6e   �� 5    
   -   5        $��txtUsername��5    N   $��3��6   $��1��6   $��2��6/   $��5q�6C   $	��310��6a   $��0q�6n   �� 5!   
   $��4��6   $��1��6   $��Password��6-   $��35 5 5 5q�6@   
�������$��Bottom=�6e   G� 5"   
   -   5        $��txtPassword��5"   R   $��5��6   $��2��6#   $��1��67   $��5q�6G   $	��310��6f   $��0q�6r   �� 5#   
   $��7��6   $��1��6#   $��2��63    �� Orientation$ 
Horizontal=�6G   $��Center=�6`   .+�5$      �� -   5        $��btnLogin��5$   1   $��5q�6   $��Log In��6    $	��150��6X   $��30��6d    � Cursor$
 Hand��6p   ���	#FF181C3E
�������$��0q�6�   �� 5%      -   5        $��btnExit��5%   0   $��5q�6   $
��Exit��6!   $	��150��6U   $��30��6a   $
 Hand��6m   
���  ��
�������$��0q�6�   5'      �� 5(   
   $$��Images/inventoryManagement.png��6   $��3��68   $��1��6H   $��7��6U   $��2��6f   �� 5)   
   -   5        $��lblError��5)   8   $��4��6   $��1��6   $��5q�6-   
���  ��$y�	Collapsed=�6Y   5*      5+      