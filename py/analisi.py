import numpy as np
import matplotlib.pyplot as plt
import io
import logging

class telemetria:

    @staticmethod
    def analizza(car_dict):

        try:
            x = np.array(car_dict['x'])
            z = np.array(car_dict['z'])
            t = np.array(car_dict['tempo'])
            v = np.array(car_dict['vel'])
            rpm = np.array(car_dict['rpm'])
            temp = np.array(car_dict['temp'])
            marce = np.array(car_dict['marcia'], dtype=int)
            
            v_ = v * 3.6
            v_media = np.mean(v_)
            v_max = np.max(v_)

            dv = np.diff(v)
            dt = np.diff(t)
            a = np.divide(dv, dt, out=np.zeros_like(dv))
            a_max = np.max(a)

            temp_media = np.mean(temp)

            rpm_max = np.max(rpm)
            marcia_top = np.bincount(marce).argmax()

            #traiettria
            fig1, ax1 = plt.subplots(figsize=(10, 7))
            ax1.plot(x, z, label='Traiettoria', color='blue', linewidth=2)
            ax1.scatter(x[0], z[0], color='green', s=100, label='Partenza', zorder=5)
            ax1.scatter(x[-1], z[-1], color='red', s=100, label='Arrivo', zorder=5)
            ax1.set_xlabel('X (m)')
            ax1.set_ylabel('Z (m)')
            ax1.set_title('Traiettoria del veicolo')
            ax1.legend()
            ax1.grid(True, linestyle='--', alpha=0.6)
            buf1 = io.BytesIO()
            fig1.savefig(buf1, format='png')
            plt.close(fig1)

            #velocita
            fig2, ax2 = plt.subplots(figsize=(10, 7))
            ax2.plot(t, v_, label='Velocità', color='purple', linewidth=1.5)
            ax2.set_ylabel('Velocità (km/h)')
            ax2.set_xlabel('Tempo (s)')
            ax2.set_title('Velocità nel tempo')
            ax2.grid(True, linestyle=':', alpha=0.7)
            buf2 = io.BytesIO()
            fig2.savefig(buf2, format='png')
            plt.close(fig2)

            return {
                "v_media": v_media, "v_max": v_max, "a_max": a_max,
                "temp_media": temp_media, "rpm_max": int(rpm_max),
                "marcia": int(marcia_top), "dist": x[-1] if x.size > 0 else 0,
                "img1": buf1.getvalue(), "img2": buf2.getvalue()
            }

        except Exception as e:
            logging.error(f"Errore analizzatore: {e}")
            return None